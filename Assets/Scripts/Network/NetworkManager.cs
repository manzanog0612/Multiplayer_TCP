using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

using UnityEngine;

[Serializable]
public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public Dictionary<MESSAGE_TYPE, int> lastMessagesIds;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp, Dictionary<MESSAGE_TYPE, int> lastMessagesIds)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.lastMessagesIds = lastMessagesIds;
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

    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();

    private int clientId = 0; // This id should be generated during first handshake

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
public Action<byte[], IPEndPoint, int> OnReceiveEvent = null;
    public Action<bool> onStartConnection = null;
    public Action<int> onAddNewClient = null;
    #endregion

    #region UNITY_CALLS
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
            case MESSAGE_TYPE.HAND_SHAKE:
                (long, int) message = new HandShakeMessage().Deserialize(data);
                AddClient(ip, timeStamp);
                ReceiveEvent();

                if (isServer)
                {
                    UdpBroadcast(new ClientsListMessage(GetClientsList()).Serialize(-1)); //admission time doesn't matter in this case because server was the originator
                }
                break;
            case MESSAGE_TYPE.CLIENTS_LIST:
                if (waitingHandShakeBack)
                {
                    (long, float)[] clientsList = new ClientsListMessage().Deserialize(data);

                    for (int i = 0; i < clientsList.Length; i++)
                    {
                        IPEndPoint client = new IPEndPoint(clientsList[i].Item1, port);
                        AddClient(client, clientsList[i].Item2);
                    }

                    waitingHandShakeBack = false;
                }
                break;
            case MESSAGE_TYPE.STRING:
            case MESSAGE_TYPE.VECTOR2:
                if (ipToId.ContainsKey((ip, timeStamp)))
                {
                    //int messageId = messageFormater.GetMessageId(data);
                    //Dictionary<MESSAGE_TYPE, int> lastMessagesIds = clients[ipToId[ip]].lastMessagesIds;
                    //
                    //if (lastMessagesIds.ContainsKey(messageType))
                    //{
                    //    if (lastMessagesIds[messageType] <= messageId)
                    //    {
                    //        ReceiveEvent();
                    //        lastMessagesIds[messageType] = messageId;
                    //    }
                    //    else
                    //    {
                    //        // Ignore message, it's old
                    //    }
                    //}
                    //else
                    //{
                    //    lastMessagesIds.Add(messageType, messageId);
                    //    ReceiveEvent();
                    //}

                    ReceiveEvent();//++
                }
                break;
            default:
                break;
        }

        void ReceiveEvent()
        {
            if (ipToId.ContainsKey((ip, timeStamp)))
            {
                OnReceiveEvent?.Invoke(data, ip, ipToId[(ip, timeStamp)]);
            }
        }
    }
    #endregion

    #region UDP
    public void StartUdpServer(int port)
    {
        isServer = true;
        this.port = port;
        udpConnection = new UdpConnection(port, this);

        onStartConnection.Invoke(isServer);

        Debug.Log("Server created");
    }

    public void StartUdpClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        udpConnection = new UdpConnection(ip, port, this);

        SendHandShake();

        onStartConnection.Invoke(isServer);
    }

    public void SendToUdpServer(byte[] data)
    {
        udpConnection.Send(data);
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

        HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port));
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
    }

    private (long, float)[] GetClientsList()
    {
        List<(long, float)> clients = new List<(long, float)>();

        foreach (var client in this.clients)
        {
            clients.Add((client.Value.ipEndPoint.Address.Address, client.Value.timeStamp));
        }

        return clients.ToArray();
    }

    private void AddClient(IPEndPoint ip, float realtimeSinceStartup)
    {
        if (!ipToId.ContainsKey((ip, realtimeSinceStartup)))
        {
            logger.SendLog("Adding client: " + ip.Address);

            int id = clientId;
            ipToId[(ip, realtimeSinceStartup)] = clientId;

            clients.Add(clientId, new Client(ip, id, realtimeSinceStartup, new Dictionary<MESSAGE_TYPE, int>()));

            onAddNewClient?.Invoke(clientId);

            clientId++;
        }
    }

    private void RemoveClient(IPEndPoint ip, float admissionTime)
    {
        if (ipToId.ContainsKey((ip, admissionTime)))
        {
            logger.SendLog("Removing client: " + ip.Address);
            clients.Remove(ipToId[(ip, admissionTime)]);
        }
    }
    #endregion
}
