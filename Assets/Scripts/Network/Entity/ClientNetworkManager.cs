using System.Net;

using UnityEngine;

public class ClientNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    private UdpConnection udpConnection = null;
    private TcpClientConnection tcpClientConnection = null;

    private double latency = 20;
    #endregion

    #region PROPERTIES
    public float admissionTimeStamp { get; private set; }
    #endregion

    #region PUBLIC_METHODS
    public override void Update()
    {
        base.Update();

        udpConnection?.FlushReceiveData();
        tcpClientConnection?.FlushReceiveData();
    }

    public void DisconectClient()
    {
        Debug.Log("This player is going to be eliminated");
        SendDisconnect(assignedId);
    }

    #region UDP
    public void StartUdpClient(IPAddress ip, int port)
    {
        isServer = false;

        NetworkManager.port = port;
        ipAddress = ip;

        udpConnection = new UdpConnection(ip, port, this);

        //SendHandShake();
        SendConnectRequest();

        onDefineIsServer?.Invoke(isServer);

        Application.quitting += DisconectClient;
    }

    public void SendToUdpServer(byte[] data)
    {
        udpConnection?.Send(data);
    }
    #endregion

    #region TCP
    public void StartTcpClient(IPAddress ip, int port)
    {
        isServer = false;

        NetworkManager.port = port;
        this.ipAddress = ip;

        tcpClientConnection = new TcpClientConnection(ip, port, this);

        SendConnectRequest();

        onDefineIsServer?.Invoke(isServer);
    }

    public void SendToTcpServer(byte[] data)
    {
        tcpClientConnection?.Send(data);
    }
    #endregion
    #endregion

    #region PROTECTED_METHODS
    protected override void AddClient(IPEndPoint ip, int clientId, float realtimeSinceStartup, Vector3 position, Color color)
    {
        base.AddClient(ip, clientId, realtimeSinceStartup, position, color);

        if (ipToId.ContainsKey((ip, realtimeSinceStartup)))
        {
            Debug.Log("Client" + assignedId.ToString() + " is adding client: " + clientId.ToString());
        }
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    protected override void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        latency = CalculateLatency(data);
    }

    protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
    {
        base.ProcessConnectRequest(ip, data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.CONNECT_REQUEST, ip);
            return;
        }

        (long server, int port) serverData = new ConnectRequestMessage().Deserialize(data);

        udpConnection.Close();
        udpConnection = null;
        udpConnection = new UdpConnection(new IPAddress(serverData.server), serverData.port, this);

        port = serverData.port;

        Debug.Log("Received connection data, now connecting to port " + port.ToString() + " and sending handshake.");

        SendHandShake();

        onStartConnection.Invoke();
    }

    protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
    {
        base.ProcessEntityDisconnect(ip, data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.ENTITY_DISCONECT, ip);
            return;
        }

        int clientId = new RemoveEntityMessage().Deserialize(data);

        if (!clients.ContainsKey(clientId))
        {
            return;
        }

        if (clientId == assignedId)
        {
            udpConnection.Close();
            Application.Quit();
        }
        else
        {
            RemoveClient(clientId);
        }
    }

    protected override void ProcessClientList(byte[] data)
    {
        base.ProcessClientList(data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.CLIENTS_LIST, null);
            return;
        }

        Debug.Log("Client" + assignedId.ToString() + " is adding processing client list");

        ((int id, long server, float timeSinceConection, Vector3 position, Color color)[] clientsList, int id) = new ClientsListMessage().Deserialize(data);

        for (int i = 0; i < clientsList.Length; i++)
        {
            IPEndPoint client = new IPEndPoint(clientsList[i].server, port);
            AddClient(client, clientsList[i].id, clientsList[i].timeSinceConection, clientsList[i].position, clientsList[i].color);
        }

        Debug.Log("Client" + assignedId.ToString() + " got his id = " + id.ToString() + " assigned for first time");
        assignedId = id;
    }
    #endregion

    #region SEND_DATA_METHODS
    public void SendPlayerMessageMessage(PlayerData playerData) //esto es horrible pero es para handlear lo que ya tenía, no tengo tiempo para arreglar arq ahora
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] message = playerDataMessage.Serialize(admissionTimeStamp);
        SendData(message);

        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(message);

        if (messageType == MESSAGE_TYPE.STRING)
        {
            SaveSentMessage(messageType, message, latency * latencyMultiplier);
        }
    }

    protected override void SendResendDataMessage(MESSAGE_TYPE messageType, IPEndPoint ip)
    {
        base.SendResendDataMessage(messageType, ip);

        ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

        byte[] message = resendDataMessage.Serialize(-1);

        SendData(message);

        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, message, latency * latencyMultiplier);
    }

    public override void SendDisconnect(int id)
    {
        base.SendDisconnect(id);

        RemoveEntityMessage removeClientMessage = new RemoveEntityMessage(id);

        if (!clients.ContainsKey(id))
        {
            return;
        }

        byte[] data = removeClientMessage.Serialize(clients[id].timeStamp);

        SendData(data);

        SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, latency * latencyMultiplier);
    }

    private void SendConnectRequest()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((client.Address.Address, client.Port));

        byte[] message = connectRequestMessage.Serialize(admissionTimeStamp);

        SendData(message);

        SaveSentMessage(MESSAGE_TYPE.CONNECT_REQUEST, message, latency * latencyMultiplier);
    }

    private void SendHandShake()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        admissionTimeStamp = Time.realtimeSinceStartup;

        HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port, new Color(RandNum(), RandNum(), RandNum(), 1)));

        byte[] message = handShakeMessage.Serialize(admissionTimeStamp);

        SendData(message);

        SaveSentMessage(MESSAGE_TYPE.HAND_SHAKE, message, latency * latencyMultiplier);
    }
    #endregion

    #region AUX
    protected override void SendData(byte[] data)
    {
        if (IsTcpConnection)
        {
            SendToTcpServer(data);
        }
        else
        {
            SendToUdpServer(data);
        }
    }

    private float RandNum()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }
    #endregion
}
