﻿using System;
using System.Collections.Generic;
using System.Net;

using UnityEngine;

public class NetworkManager : IReceiveData
{
    #region INSTANCE
    public static NetworkManager Instance = null;
    #endregion

    #region PROTECTED_FIELDS   
    protected int clientId = 0;
    
    protected Dictionary<MESSAGE_TYPE, (byte[] data, float time)> lastSemiTcpMessages = new Dictionary<MESSAGE_TYPE, (byte[], float)>();

    protected bool wasLastMessageSane = true;

    protected Dictionary<int, Client> clients = new Dictionary<int, Client>();
    protected readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();
    #endregion

    #region PROPERTIES
    public IPAddress ipAddress { get; protected set; }
    public static int port { get; set; }
    public bool isServer { get; protected set; }
    public bool IsTcpConnection => isTcpConnection;
    public int assignedId { get; protected set; }
    #endregion

    #region ACTIONS
    public Action<byte[], IPEndPoint, int, MESSAGE_TYPE> onReceiveEvent = null;
    public Action onStartConnection = null;
    public Action<bool> onDefineIsServer = null;
    public Action<int, (long, float), Vector3, Color> onAddNewClient = null;
    public Action<int> onRemoveClient = null;
    public Action<int> onSync = null;
    #endregion

    #region CONSTANTS
    private const bool isTcpConnection = false;
    protected const double latencyMultiplier = 5;
    #endregion

    #region PUBLIC_METHODS
    public virtual void Update()
    {
        UpdateSavedMessagesTimes();
    }

    public virtual void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);
        float timeStamp = MessageFormater.GetAdmissionTime(data);

        switch (messageType)
        {
            case MESSAGE_TYPE.SYNC:
                ProcessSync((ip, timeStamp), data);
                break;
            case MESSAGE_TYPE.RESEND_DATA:
                ProcessResendData(ip, data);
                break;
            case MESSAGE_TYPE.SERVER_DATA_UPDATE:
                ProcessServerDataUpdate(ip, data);
                break;
            case MESSAGE_TYPE.SERVER_ON:
                ProcessServerOn(ip, data);
                break;
            case MESSAGE_TYPE.CONNECT_REQUEST:
                ProcessConnectRequest(ip, data);
                break;
            case MESSAGE_TYPE.ENTITY_DISCONECT:
                ProcessEntityDisconnect(ip, data);//
                break;
            case MESSAGE_TYPE.HAND_SHAKE:
                ProcessHandShake((ip, timeStamp), data);//
                break;
            case MESSAGE_TYPE.CLIENTS_LIST:
                ProcessClientList(data);
                break;
            case MESSAGE_TYPE.STRING:
            case MESSAGE_TYPE.VECTOR3:
                ProcessGameMessage((ip, timeStamp), data, messageType);//
                break;
            default:
                break;
        }
    }

    public virtual void SendDisconnect(int id)
    {
        Debug.Log("Removing player " + id.ToString());
    }
    #endregion

    #region PROTECTED_METHODS

    protected virtual void AddClient(IPEndPoint ip, int clientId, float realtimeSinceStartup, Vector3 position, Color color)
    {
        if (ipToId.ContainsKey((ip, realtimeSinceStartup)))
        {
            return;
        }

        ipToId[(ip, realtimeSinceStartup)] = clientId;

        clients.Add(clientId, new Client(ip, clientId, realtimeSinceStartup, new Dictionary<MESSAGE_TYPE, int>(), position, color));

        onAddNewClient?.Invoke(clientId, (ip.Address.Address, realtimeSinceStartup), position, color);        
    }

    protected virtual void RemoveClient(int id)
    {
        if (!clients.ContainsKey(id))
        {
            return;
        }

        onRemoveClient?.Invoke(id);

        Debug.Log("Removing client: " + id);
        ipToId.Remove((clients[id].ipEndPoint, clients[id].timeStamp));
        clients.Remove(id);        
    }

    protected double CalculateLatency(byte[] data)
    {
        (int day, int hour, int minute, int second, int millisecond) sendTime = MessageFormater.GetMessageSendTime(data);

        DateTime utcNow = new DateTime(2023, 5, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
        DateTime messageNow = new DateTime(2023, 5, sendTime.day, sendTime.hour, sendTime.minute, sendTime.second, sendTime.millisecond);

        double latency = (int)(utcNow - messageNow).TotalMilliseconds;

        Debug.Log("L: " + latency.ToString());

        return latency;
    }

    protected bool CheckMessageSanity(byte[] data, int headerSize, int messageSize, SemiTcpMessage semiTcpMessage, int op)
    {
        MessageTail messageTail = semiTcpMessage.DeserializeTail(data, headerSize, messageSize);

        return op == messageTail.messageOperationResult && data.Length == messageTail.messageSize;
    }

    protected void SaveSentMessage(MESSAGE_TYPE messageType, byte[] data, double saveTime)
    {
        if (lastSemiTcpMessages.ContainsKey(messageType))
        {
            lastSemiTcpMessages[messageType] = (data, (float)saveTime / 1000);
        }
        else
        {
            lastSemiTcpMessages.Add(messageType, (data, (float)saveTime / 1000));
        }
    }

    protected virtual void SendResendDataMessage(MESSAGE_TYPE messageType, IPEndPoint ip)
    {
        Debug.Log("RESENDING DATA " + (int)messageType);
    }

    protected virtual void SendData(byte[] data)
    {

    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    private void a(int a)
    {
        if (!wasLastMessageSane)
        {
            Debug.Log("EL MENSAJE " + a + " ESTABA INSANO");
        }
    }

    protected virtual void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data) 
    {
        if (isServer)
        {
            if (!ipToId.ContainsKey(clientConnectionData))
            {
                return;
            }
        }

        int id = isServer ? ipToId[clientConnectionData] : -1;

        onSync?.Invoke(id);
    }

    protected virtual void ProcessResendData(IPEndPoint ip, byte[] data)
    {
        MESSAGE_TYPE messageTypeToResend = new ResendDataMessage().Deserialize(data);
        ResendDataMessage resendDataMessage = new ResendDataMessage(messageTypeToResend);

        wasLastMessageSane = CheckMessageSanity(data, resendDataMessage.GetHeaderSize(), resendDataMessage.GetMessageSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.RESEND_DATA);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.RESEND_DATA, ip);
            return;
        }

        Debug.Log("Received the sign to resend data " + (int)messageTypeToResend);

        if (lastSemiTcpMessages.ContainsKey(messageTypeToResend))
        {
            SendData(lastSemiTcpMessages[messageTypeToResend].data);
        }
        else
        {
            Debug.Log("There wasn't data of message type " + (int)messageTypeToResend);
        }
    }

    protected virtual void ProcessServerDataUpdate(IPEndPoint ip, byte[] data)
    {
        ServerDataUpdateMessage serverDataUpdateMessage = new ServerDataUpdateMessage(new ServerDataUpdateMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, serverDataUpdateMessage.GetHeaderSize(), serverDataUpdateMessage.GetMessageSize(), serverDataUpdateMessage, serverDataUpdateMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.SERVER_DATA_UPDATE);
    }

    protected virtual void ProcessServerOn(IPEndPoint ip, byte[] data)
    {
        ServerOnMessage serverOnMessage = new ServerOnMessage(new ServerOnMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, serverOnMessage.GetHeaderSize(), serverOnMessage.GetMessageSize(), serverOnMessage, serverOnMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.SERVER_ON);
    }

    protected virtual void ProcessConnectRequest(IPEndPoint ip, byte[] data) 
    {
        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage(new ConnectRequestMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, connectRequestMessage.GetHeaderSize(), connectRequestMessage.GetMessageSize(), connectRequestMessage, connectRequestMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.CONNECT_REQUEST);
    }
    
    protected virtual void ProcessEntityDisconnect(IPEndPoint ip, byte[] data) 
    {
        RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(new RemoveEntityMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, removeEntityMessage.GetHeaderSize(), removeEntityMessage.GetMessageSize(), removeEntityMessage, removeEntityMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.ENTITY_DISCONECT);
    }
    
    protected virtual void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data) 
    {
        HandShakeMessage handShakeMessage = new HandShakeMessage(new HandShakeMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, handShakeMessage.GetHeaderSize(), handShakeMessage.GetMessageSize(), handShakeMessage, handShakeMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.HAND_SHAKE);
    }
    
    protected virtual void ProcessClientList(byte[] data) 
    {
        ClientsListMessage clientsListMessage = new ClientsListMessage(new ClientsListMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, clientsListMessage.GetHeaderSize(), clientsListMessage.GetMessageSize(), clientsListMessage, clientsListMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.CLIENTS_LIST);
    }
    
    protected virtual void ProcessGameMessage((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        Debug.Log("Received data from client " + ipToId[clientConnectionData]);

        wasLastMessageSane = true;

        switch (messageType)
        {
            case MESSAGE_TYPE.STRING:
                StringMessage stringMessage = new StringMessage(new StringMessage().Deserialize(data));

                wasLastMessageSane = CheckMessageSanity(data, stringMessage.GetHeaderSize(), stringMessage.GetMessageSize(), stringMessage, stringMessage.GetMessageTail().messageOperationResult);
                
                if (!wasLastMessageSane)
                {
                    SendResendDataMessage(MESSAGE_TYPE.STRING, clientConnectionData.ip);
                    return;
                }
                else
                {
                    OnReceiveGameEvent(clientConnectionData, data, messageType);
                }
                break;
            case MESSAGE_TYPE.VECTOR3:
                CheckIfMessageIdIsCorrect(clientConnectionData, data, messageType);
                break;
            default:
                break;
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void UpdateSavedMessagesTimes()
    {
        Dictionary<MESSAGE_TYPE, float> lastMessageTimes = new Dictionary<MESSAGE_TYPE, float>();

        foreach (var messages in lastSemiTcpMessages)
        {
            lastMessageTimes.Add(messages.Key, messages.Value.time - Time.deltaTime);
        }

        foreach (var messageTime in lastMessageTimes)
        {
            if (lastMessageTimes[messageTime.Key] < 0)
            {
                lastSemiTcpMessages.Remove(messageTime.Key);
            }
            else
            { 
                lastSemiTcpMessages[messageTime.Key] = (lastSemiTcpMessages[messageTime.Key].data, messageTime.Value);
            }
        }
    }
    #endregion

    #region AUX
    protected virtual void OnReceiveGameEvent((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        int id = ipToId[clientConnectionData];

        if (messageType == MESSAGE_TYPE.VECTOR3)
        {
            clients[id].position = new Vector3Message().Deserialize(data);
        }

        onReceiveEvent?.Invoke(data, clientConnectionData.ip, id, messageType);
    }
    
    private void CheckIfMessageIdIsCorrect((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        int clientId = ipToId[clientConnectionData];

        int messageId = MessageFormater.GetMessageId(data);
        Dictionary<MESSAGE_TYPE, int> lastMessagesIds = clients[clientId].lastMessagesIds;

        if (lastMessagesIds.ContainsKey(messageType))
        {
            if (lastMessagesIds[messageType] <= messageId)
            {
                lastMessagesIds[messageType] = messageId;
                OnReceiveGameEvent(clientConnectionData, data, messageType);
            }
            else
            {
                Debug.Log("Received old message, was erased");
                wasLastMessageSane = false;
            }
        }
        else
        {
            lastMessagesIds.Add(messageType, messageId);
            OnReceiveGameEvent(clientConnectionData, data, messageType);
        }
    }
    #endregion
}
