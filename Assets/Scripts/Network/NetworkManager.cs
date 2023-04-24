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

    protected Dictionary<int, Client> clients = new Dictionary<int, Client>();
    protected readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();
    #endregion

    #region PROPERTIES
    public IPAddress ipAddress { get; protected set; }
    public static int port { get; protected set; }
    public bool isServer { get; protected set; }
    public bool IsTcpConnection => isTcpConnection;
    #endregion

    #region ACTIONS
    public Action<byte[], IPEndPoint, int> onReceiveEvent = null;
    public Action<int> onReceiveServerSyncMessage = null;
    public Action<bool> onStartConnection = null;
    public Action<int, (long, float), Vector3, Color> onAddNewClient = null;
    public Action onSendData = null;
    public Action<int> onRemoveClient = null;
    #endregion

    #region CONSTANTS
    private const bool isTcpConnection = false;
    #endregion

    #region PUBLIC_METHODS
    public virtual void Update()
    {
    }

    public virtual void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);
        float timeStamp = MessageFormater.GetAdmissionTime(data);

        switch (messageType)
        {
            case MESSAGE_TYPE.CONNECT_REQUEST:
                ProcessConnectRequest(ip, data);
                break;
            case MESSAGE_TYPE.CLIENT_DISCONECT:
                ProcessRemoveClient(data);
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
        RemoveClientMessage removeClientMessage = new RemoveClientMessage(id);

        if (!clients.ContainsKey(id))
        {
            return;
        }

        byte[] data = removeClientMessage.Serialize(clients[id].timeStamp);

        DataHandler.Instance.SendData(data);

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

    protected void RemoveClient(int id)
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

        onReceiveEvent?.Invoke(data, clientConnectionData.ip, id);
    }
    #endregion

    protected virtual void ProcessConnectRequest(IPEndPoint ip, byte[] data) {}
    
    protected virtual void ProcessRemoveClient(byte[] data) {}
    
    protected virtual void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data) {}
    
    protected virtual void ProcessClientList(byte[] data) {}
    
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
}
