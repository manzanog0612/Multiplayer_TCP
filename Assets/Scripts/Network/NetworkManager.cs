using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public Dictionary<MESSAGE_TYPE, int> lastMessagesIds;
    public Vector3 position = Vector3.zero;
    public Color color = Color.white;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp, Dictionary<MESSAGE_TYPE, int> lastMessagesIds, Vector3 position, Color color)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.lastMessagesIds = lastMessagesIds;
        this.position = position;
        this.color = color;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    #region EXPOSED_FIELDS
    [SerializeField] private bool tcpConnection = true;
    [SerializeField] private int timeOut = 30;
    [SerializeField] private Logger logger = null;
    #endregion

    #region PRIVATE_FIELDS
    private UdpConnection udpConnection = null;
    private TcpClientConnection tcpClientConnection = null;
    private TcpServerConnection tcpServerConnection = null;

    private int assignedId = 0;

    private int clientId = 0;

    private Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();

    private MessageFormater messageFormater = new MessageFormater();

    private bool waitingHandShakeBack = false;
    #endregion

    #region PROPERTIES
    public IPAddress ipAddress { get; private set; }
    public int port { get; private set; }
    public bool isServer { get; private set; }
    public bool TcpConnection { get; }
    public float admissionTimeStamp { get; private set; }
    #endregion

#region ACTIONS
public Action<byte[], IPEndPoint, int> onReceiveEvent = null;
    public Action<int> onReceiveServerSyncMessage = null;
    public Action<bool> onStartConnection = null;
    public Action<int, (long, float), Vector3, Color> onAddNewClient = null;
    public Action onSendData = null;
    public Action<int> onRemoveClient = null;
    #endregion

    #region UNITY_CALLS
#if UNITY_SERVER
    private void Start()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8053;

        if (tcpConnection)
        {
            StartTcpServer(ipAddress, port);
        }
        else
        {
            StartUdpServer(port);
        }
    }
#endif
    private void Update()
    {
        // Flush the data in main thread
        tcpClientConnection?.FlushReceiveData();
        tcpServerConnection?.FlushReceiveData();
        udpConnection?.FlushReceiveData();
    }
    #endregion

    #region PUBLIC_METHODS
    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MESSAGE_TYPE messageType = messageFormater.GetMessageType(data);
        float timeStamp = messageFormater.GetAdmissionTime(data);

        switch (messageType)
        {
            case MESSAGE_TYPE.CLIENT_DISCONECT:
                ProcessRemoveClient((ip, timeStamp), data);
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
    #endregion

    #region UDP
    public void StartUdpServer(int port)
    {
        isServer = true;
        this.port = port;
        udpConnection = new UdpConnection(port, this);

        onStartConnection?.Invoke(isServer);

        Debug.Log("Server created");

        Application.quitting += ShutDownUdpServer;
    }

    public void StartUdpClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        udpConnection = new UdpConnection(ip, port, this);

        SendHandShake();

        onStartConnection.Invoke(isServer);

        Application.quitting += DisconectUpdClient;
    }

    public void SendToUdpServer(byte[] data)
    {
        udpConnection.Send(data);

        onSendData?.Invoke();
    }

    public void UdpBroadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                udpConnection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }

        onSendData?.Invoke();
    }

    public void KickPlayer(int id, bool closeApp = true)
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

    public void DisconectUpdClient()
    {
        Debug.Log("This player is going to be eliminated");
        KickPlayer(assignedId, true);
    }

    public void ShutDownUdpServer()
    {
        Debug.Log("Shutting down server");
        int[] clientsIds = new int[clients.Count];

        int index = 0;

        foreach (var client in clients)
        {
            clientsIds[index] = client.Key;
            index++;
        }

        for (int i = 0; i < index; i++)
        {
            KickPlayer(clientsIds[i], false);
        }
    }
    #endregion

    #region TCP
    public void StartTcpServer(IPAddress ip, int port)
    {
        isServer = true;
        this.port = port;
        tcpServerConnection = new TcpServerConnection(ip, port, this, logger);

        onStartConnection.Invoke(isServer);
    }

    public void StartTcpClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        tcpClientConnection = new TcpClientConnection(ip, port, this, logger);

        SendHandShake();

        onStartConnection.Invoke(isServer);
    }

    public void SendToTcpServer(byte[] data)
    {
        tcpClientConnection?.Send(data);
    }

    public void TcpBroadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                tcpServerConnection.Send(data);
            }
        }
    }
    #endregion

    #region PRIVATE_METHODS   
    private IEnumerator OnWaitForHandShake()
    {
        int maxWaitHandShakeBackTime = 5;
        float time = 0;

        while (time < maxWaitHandShakeBackTime && waitingHandShakeBack)
        {
            time += Time.deltaTime;

            yield return null;
        }

        if (waitingHandShakeBack)
        {
            SendHandShake();
        }
    }

    private void SendHandShake()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        admissionTimeStamp = Time.realtimeSinceStartup;

        HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port, new Color(RandNum(), RandNum(), RandNum(), 1)));
        waitingHandShakeBack = true;
        
        if (tcpConnection)
        {
            SendToTcpServer(handShakeMessage.Serialize(admissionTimeStamp));
        }
        else
        {
            SendToUdpServer(handShakeMessage.Serialize(admissionTimeStamp));
        }

        StartCoroutine(OnWaitForHandShake());

        float RandNum()
        {
            return UnityEngine.Random.Range(0f, 1f);
        }
    }

    private (int, long, float, Vector3, Color)[] GetClientsList()
    {
        List<(int, long, float, Vector3, Color)> clients = new List<(int, long, float, Vector3, Color)>();

        foreach (var client in this.clients)
        {
            clients.Add((client.Value.id, client.Value.ipEndPoint.Address.Address, client.Value.timeStamp, client.Value.position, client.Value.color));
        }

        return clients.ToArray();
    }

    private void AddClient(IPEndPoint ip, int clientId, float realtimeSinceStartup, Vector3 position, Color color)
    {
        if (!ipToId.ContainsKey((ip, realtimeSinceStartup)))
        {
            int id = isServer ? this.clientId : clientId;
            ipToId[(ip, realtimeSinceStartup)] = id;

            clients.Add(id, new Client(ip, id, realtimeSinceStartup, new Dictionary<MESSAGE_TYPE, int>(), position, color));

            onAddNewClient?.Invoke(id, (ip.Address.Address, realtimeSinceStartup), position, color);

            Debug.Log((isServer ? "Server" : "Client" + assignedId.ToString()) + " is adding client: " + id.ToString());

            if (isServer)
            { 
                this.clientId++; 
            }
        }
    }

    private void RemoveClient(int id)
    {
        if (clients.ContainsKey(id))
        {
            onRemoveClient?.Invoke(id);

            Debug.Log("Removing client: " + id);
            ipToId.Remove((clients[id].ipEndPoint, clients[id].timeStamp));
            clients.Remove(id);
        }
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

    #endregion
    private void OnReceiveEvent((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
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

        if (isServer)
        {
            onReceiveServerSyncMessage?.Invoke(id);
        }

        onReceiveEvent?.Invoke(data, clientConnectionData.ip, id);        
    }

    private void ProcessRemoveClient((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        int clientId = new RemoveClientMessage().Deserialize(data);

        if (!clients.ContainsKey(clientId))
        {
            return;
        }

        if (isServer)
        {
            Debug.Log("Server is sendong the signal to eliminate client: " + clientId.ToString());

            if (tcpConnection)
            {
                TcpBroadcast(new RemoveClientMessage(clientId).Serialize(-1));
            }
            else
            {
                UdpBroadcast(new RemoveClientMessage(clientId).Serialize(-1)); //admission time doesn't matter in this case because server was the originator
            }
        }
        else
        {
            if (clientId == assignedId)
            {
                Application.Quit();
            }
            else
            {
                RemoveClient(clientId);
            }
        }
    }

    private void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        if (isServer)
        {
            Debug.Log("Server is processing Handshake");

            (long ip, int port, Color color) message = new HandShakeMessage().Deserialize(data);
            AddClient(clientConnectionData.ip, clientId - 1, clientConnectionData.timeStamp, Vector3.zero, message.color);
            OnReceiveEvent(clientConnectionData, data, MESSAGE_TYPE.HAND_SHAKE);

            if (tcpConnection)
            {
                TcpBroadcast(new ClientsListMessage((GetClientsList(), clientId - 1)).Serialize(-1));
            }
            else
            {
                UdpBroadcast(new ClientsListMessage((GetClientsList(), clientId - 1)).Serialize(-1)); //admission time doesn't matter in this case because server was the originator
            }
        }
    }

    private void ProcessClientList(byte[] data)
    {
        Debug.Log("Client" + assignedId.ToString()+ " is adding processing client list");

        ((int id, long server, float timeSinceConection, Vector3 position, Color color)[] clientsList, int id) = new ClientsListMessage().Deserialize(data);

        for (int i = 0; i < clientsList.Length; i++)
        {
            IPEndPoint client = new IPEndPoint(clientsList[i].server, port);
            AddClient(client, clientsList[i].id, clientsList[i].timeSinceConection, clientsList[i].position, clientsList[i].color);
        }

        if (!waitingHandShakeBack)
        {
            return;
        }

        Debug.Log("Client" + assignedId.ToString() + "got his id = " + id.ToString() + " assigned for first time");
        assignedId = id;

        waitingHandShakeBack = false;        
    }

    private void ProcessGameMessage((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (IfSyncProcess(data, messageType) && !ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        Debug.Log("Received data from client " + ipToId[clientConnectionData]);

        int messageId = messageFormater.GetMessageId(data);
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
