using System;
using System.Collections.Generic;
using System.Net;

using UnityEngine;

public class NetworkManager : IReceiveData
{
    #region INSTANCE
    public static NetworkManager Instance = null;
    #endregion

    #region PROTECTED_FIELDS
    protected int assignedId = 0;
    protected int clientId = 0;

    protected double latency = 0;
    
    protected Dictionary<MESSAGE_TYPE, (byte[], float)> lastSemiTcpMessages = new Dictionary<MESSAGE_TYPE, (byte[], float)>();

    protected bool wasLastMessageSane = true;

    protected Dictionary<int, Client> clients = new Dictionary<int, Client>();
    protected readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();
    #endregion

    #region PROPERTIES
    public IPAddress ipAddress { get; protected set; }
    public static int port { get; set; }
    public bool isServer { get; protected set; }
    public bool IsTcpConnection => isTcpConnection;
    #endregion

    #region ACTIONS
    public Action<byte[], IPEndPoint, int, MESSAGE_TYPE> onReceiveEvent = null;
    public Action<int> onReceiveServerSyncMessage = null;
    public Action onStartConnection = null;
    public Action<bool> onDefineIsServer = null;
    public Action<int, (long, float), Vector3, Color> onAddNewClient = null;
    public Action onSendData = null;
    public Action<int> onRemoveClient = null;
    #endregion

    #region CONSTANTS
    private const bool isTcpConnection = false;
    protected const double timeTillEraseLastMessage = 33 * 5;
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

        CalculateLatency(data);

        switch (messageType)
        {
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
                ProcessEntityDisconnect(ip, data);
                break;
            case MESSAGE_TYPE.HAND_SHAKE:
                ProcessHandShake((ip, timeStamp), data);
                break;
            case MESSAGE_TYPE.CLIENTS_LIST:
                ProcessClientList(data);
                break;
            case MESSAGE_TYPE.STRING:
            case MESSAGE_TYPE.VECTOR3:
                ProcessGameMessage((ip, timeStamp), data, messageType);
                break;
            default:
                break;
        }
    }

    public void KickClient(int id, bool closeApp = true)
    {
        Debug.Log("Removing player " + id.ToString());
        RemoveEntityMessage removeClientMessage = new RemoveEntityMessage(id);

        if (!clients.ContainsKey(id))
        {
            return;
        }

        byte[] data = removeClientMessage.Serialize(clients[id].timeStamp);

        DataHandler.Instance.SendData(data);

        SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data);

        if (closeApp)
        {
            Application.Quit();
        }
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

    protected virtual void CalculateLatency(byte[] data)
    {
        (int day, int hour, int minute, int second, int millisecond) sendTime = MessageFormater.GetMessageSendTime(data);

        DateTime utcNow = new DateTime(2023, 5, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
        DateTime messageNow = new DateTime(2023, 5, sendTime.day, sendTime.hour, sendTime.minute, sendTime.second, sendTime.millisecond);

        latency = (int)(utcNow - messageNow).TotalMilliseconds;

        Debug.Log("L: " + latency.ToString());
    }

    protected virtual bool CheckMessageSanity(byte[] data, int headerSize, SemiTcpMessage semiTcpMessage, float op)
    {
        MessageTail messageTail = semiTcpMessage.DeserializeTail(data, headerSize);

        return op == messageTail.messageOperationResult;
    }

    protected virtual void SaveSentMessage(MESSAGE_TYPE messageType, byte[] data)
    {
        if (lastSemiTcpMessages.ContainsKey(messageType))
        {
            lastSemiTcpMessages[messageType] = (data, 0);
        }
        else
        {
            lastSemiTcpMessages.Add(messageType, (data, 0));
        }
    }

    protected virtual void SendResendDataMessage(MESSAGE_TYPE messageType)
    {
        ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

        byte[] message = resendDataMessage.Serialize(-1);

        Debug.Log("RESENDING DATA " + (int)messageType);

        SendData(message);

        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, message);
    }

    protected virtual void SendData(byte[] data)
    {

    }

    #region DATA_RECEIVE_PROCESS
    #region AUX
    private bool IfSyncProcess(byte[] data, MESSAGE_TYPE messageType)
    {
        if (!isServer && messageType == SyncHandler.serverSyncMessageType)
        {
            string message = new StringMessage().Deserialize(data);

            if (message == SyncHandler.serverSyncMessage)
            {
                onReceiveServerSyncMessage?.Invoke(-1);
                return true;
            }
        }

        return false;
    }

    protected virtual void OnReceiveEvent((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
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
    #endregion

    private void a(int a)
    {
        if (!wasLastMessageSane)
        {
            Debug.Log("EL MENSAJE " + a + " ESTABA INSANO");
        }
    }

    protected virtual void ProcessResendData(IPEndPoint ip, byte[] data)
    {
        ResendDataMessage resendDataMessage = new ResendDataMessage(new ResendDataMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, resendDataMessage.GetHeaderSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.RESEND_DATA);
    }

    protected virtual void ProcessServerDataUpdate(IPEndPoint ip, byte[] data)
    {
        ServerDataUpdateMessage serverDataUpdateMessage = new ServerDataUpdateMessage(new ServerDataUpdateMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, serverDataUpdateMessage.GetHeaderSize(), serverDataUpdateMessage, serverDataUpdateMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.SERVER_DATA_UPDATE);
    }

    protected virtual void ProcessServerOn(IPEndPoint ip, byte[] data)
    {
        ServerOnMessage serverOnMessage = new ServerOnMessage(new ServerOnMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, serverOnMessage.GetHeaderSize(), serverOnMessage, serverOnMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.SERVER_ON);
    }

    protected virtual void ProcessConnectRequest(IPEndPoint ip, byte[] data) 
    {
        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage(new ConnectRequestMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, connectRequestMessage.GetHeaderSize(), connectRequestMessage, connectRequestMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.CONNECT_REQUEST);
    }
    
    protected virtual void ProcessEntityDisconnect(IPEndPoint ip, byte[] data) 
    {
        RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(new RemoveEntityMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, removeEntityMessage.GetHeaderSize(), removeEntityMessage, removeEntityMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.ENTITY_DISCONECT);
    }
    
    protected virtual void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data) 
    {
        HandShakeMessage handShakeMessage = new HandShakeMessage(new HandShakeMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, handShakeMessage.GetHeaderSize(), handShakeMessage, handShakeMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.HAND_SHAKE);
    }
    
    protected virtual void ProcessClientList(byte[] data) 
    {
        ClientsListMessage clientsListMessage = new ClientsListMessage(new ClientsListMessage().Deserialize(data));

        wasLastMessageSane = CheckMessageSanity(data, clientsListMessage.GetHeaderSize(), clientsListMessage, clientsListMessage.GetMessageTail().messageOperationResult);
        a((int)MESSAGE_TYPE.CLIENTS_LIST);
    }
    
    protected virtual void ProcessGameMessage((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (IfSyncProcess(data, messageType) || !ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        Debug.Log("Received data from client " + ipToId[clientConnectionData]);

        int messageId = MessageFormater.GetMessageId(data);
        int clientId = ipToId[clientConnectionData];
        Dictionary<MESSAGE_TYPE, int> lastMessagesIds = clients[clientId].lastMessagesIds;

        wasLastMessageSane = true;

        if (lastMessagesIds.ContainsKey(messageType))
        {
            if (lastMessagesIds[messageType] <= messageId)
            {
                lastMessagesIds[messageType] = messageId;
                OnReceiveEvent(clientConnectionData, data, messageType);
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
            OnReceiveEvent(clientConnectionData, data, messageType);
        }
    }
    #endregion
    #endregion

    #region PRIVATE_METHODS
    private void UpdateSavedMessagesTimes()
    {
        Dictionary<MESSAGE_TYPE, float> lastMessageTimes = new Dictionary<MESSAGE_TYPE, float>();

        foreach (var messages in lastSemiTcpMessages)
        {
            lastMessageTimes.Add(messages.Key, messages.Value.Item2 + Time.deltaTime);
        }

        foreach (var messages in lastMessageTimes)
        {
            if (lastMessageTimes[messages.Key] > timeTillEraseLastMessage)
            {
                lastSemiTcpMessages.Remove(messages.Key);
            }
            else
            { 
                lastSemiTcpMessages[messages.Key] = (lastSemiTcpMessages[messages.Key].Item1, messages.Value);
            }
        }
    }
    #endregion
}
