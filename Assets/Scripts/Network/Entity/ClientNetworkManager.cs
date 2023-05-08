using System.Net;
using UnityEngine;

public class ClientNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    private UdpConnection udpConnection = null;
    private TcpClientConnection tcpClientConnection = null;

    private double latency = 20;

    private bool aaaaa = false;
    private bool bbbbb = false;
    private bool ccccc = true;
    private bool ddddd = false;
    private bool eeeee = false;
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

        SendHandShake();
        //SendConnectRequest();

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
        base.ProcessSync(clientConnectionData, data);

        latency = CalculateLatency(data);
        latency = latency > 20 ? latency : 0;
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

        onStartConnection.Invoke();
    }
    #endregion

    #region SEND_DATA_METHODS
    public void SendPlayerDataMessage(PlayerData playerData) //esto es horrible pero es para handlear lo que ya ten�a, no tengo tiempo para arreglar arq ahora
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] data = playerDataMessage.Serialize(admissionTimeStamp);
        //SendData(data);

        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);

        if (messageType == MESSAGE_TYPE.STRING)
        {
            SaveSentMessage(messageType, data, latency * latencyMultiplier);
        }

        if (aaaaa)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            aaaaa = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
    }

    protected override void SendResendDataMessage(MESSAGE_TYPE messageType, IPEndPoint ip)
    {
        base.SendResendDataMessage(messageType, ip);

        ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

        byte[] data = resendDataMessage.Serialize(-1);

        // SendData(data);

        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, latency * latencyMultiplier);

        if (bbbbb)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            bbbbb = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
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

        //SendData(data);

        SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, latency * latencyMultiplier);

        if (ccccc)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            ccccc = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
    }

    private void SendConnectRequest()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((client.Address.Address, client.Port));

        byte[] data = connectRequestMessage.Serialize(admissionTimeStamp);

        //SendData(data);

        SaveSentMessage(MESSAGE_TYPE.CONNECT_REQUEST, data, latency * latencyMultiplier);

        if (ddddd)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }
            
            data2[data.Length - 10] -= 1;
            ddddd = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }        
    }

    private void SendHandShake()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        admissionTimeStamp = Time.realtimeSinceStartup;

        HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port, new Color(RandNum(), RandNum(), RandNum(), 1)));

        byte[] data = handShakeMessage.Serialize(admissionTimeStamp);

        //SendData(data);

        SaveSentMessage(MESSAGE_TYPE.HAND_SHAKE, data, latency * latencyMultiplier);

        if (eeeee)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            eeeee = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
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
        return Random.Range(0f, 1f);
    }
    #endregion
}
