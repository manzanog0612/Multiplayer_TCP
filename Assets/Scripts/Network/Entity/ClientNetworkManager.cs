using System;
using System.Net;

using UnityEngine;

public class ClientNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    private UdpConnection udpConnection = null;
    private TcpClientConnection tcpClientConnection = null;

    private double latency = minimunSaveTime;

    private bool sendPlayerDataWrong = false;
    private bool sendResendDataWrong = false;
    private bool sendDiconnectClientWrong = false;
    private bool sendConnectRequestWrong = false;
    private bool sendHandShakeWrong = false;
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
        SendDisconnectClient(assignedId);
        Application.Quit();
    }

    #region UDP
    public void StartUdpClient(IPAddress ip, int port)
    {
        isServer = false;

        NetworkManager.port = port;
        ipAddress = ip;

        udpConnection = new UdpConnection(ip, port, this);

        SendHandShake();
       // SendConnectRequest();

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
    protected override void ProcessReflectionMessage(IPEndPoint ip, byte[] data)
    {
        base.ProcessReflectionMessage(ip, data);

        int clientId = ReflectionMessageFormater.GetClientId(data);
        int datasAmount = ReflectionMessageFormater.GetDatasAmount(data);

        int offset = sizeof(bool) + sizeof(int) * 2;

        for (int i = 0; i < datasAmount; i++)
        {
            int sizeOfName = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            string name = string.Empty;

            for (int j = 0; j < sizeOfName * sizeof(char); j+=sizeof(char))
            {            
                char c = BitConverter.ToChar(data, offset);
                name += c;
                offset += sizeof(char);
            }

            Debug.Log(name);

            VALUE_TYPE valueType = (VALUE_TYPE)BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            object value = null;

            switch (valueType)
            {
                case VALUE_TYPE.INT:
                    value = BitConverter.ToInt32(data, offset);
                    offset += sizeof(int);
                    break;
                case VALUE_TYPE.FLOAT:
                    value = BitConverter.ToSingle(data, offset);
                    offset += sizeof(float);
                    break;
                case VALUE_TYPE.DOUBLE:
                    value = BitConverter.ToDouble(data, offset);
                    offset += sizeof(double);
                    break;
                case VALUE_TYPE.CHAR:
                    value = BitConverter.ToChar(data, offset);
                    offset += sizeof(char);
                    break;
                case VALUE_TYPE.BOOL:
                    value = BitConverter.ToBoolean(data, offset);
                    offset += sizeof(bool);
                    break;
                case VALUE_TYPE.VECTOR3:
                    Vector3 vector3Value = Vector3.zero;
                    vector3Value.x = BitConverter.ToSingle(data, offset);
                    vector3Value.y = BitConverter.ToSingle(data, offset + sizeof(float));
                    vector3Value.z = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
                    value = vector3Value;
                    offset += sizeof(float) * 3;
                    break;
                default:
                    break;
            }

            if (value != null)
            {
                onReceiveGameEvent.Invoke(value, clientId, valueType);
            }
        }
    }

    protected override void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        base.ProcessSync(clientConnectionData, data);

        latency = CalculateLatency(data);
        latency = latency < minimunSaveTime ? minimunSaveTime : latency;
    }

    protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
    {
        base.ProcessConnectRequest(ip, data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.CONNECT_REQUEST, ip);
            return;
        }

        (long server, int port) serverData = ConnectRequestMessage.Deserialize(data);

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

        int clientId = RemoveEntityMessage.Deserialize(data);

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

        ((int id, long server, float timeSinceConection, Vector3 position, Color color)[] clientsList, int id) = ClientsListMessage.Deserialize(data);

        for (int i = 0; i < clientsList.Length; i++)
        {
            IPEndPoint client = new IPEndPoint(clientsList[i].server, port);
            AddClient(client, clientsList[i].id, clientsList[i].timeSinceConection, clientsList[i].position, clientsList[i].color);
        }

        Debug.Log("Client" + assignedId.ToString() + " got his id = " + id.ToString() + " assigned for first time");
        assignedId = id;

        onStartConnection.Invoke();
    }

    protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        base.ProcessHandShake(clientConnectionData, data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.HAND_SHAKE, clientConnectionData.ip);
            return;
        }

        Debug.Log("Server is processing Handshake");

        (long ip, int id, Color color) message = HandShakeMessage.Deserialize(data);
        AddClient(clientConnectionData.ip, message.id, clientConnectionData.timeStamp, Vector3.zero, message.color);
    }
    #endregion

    #region SEND_DATA_METHODS
    public void SendPlayerDataMessage(PlayerData playerData) //esto es horrible pero es para handlear lo que ya tenía, no tengo tiempo para arreglar arq ahora
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] data = playerDataMessage.Serialize(admissionTimeStamp);

        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);

        if (messageType == MESSAGE_TYPE.STRING)
        {
            SaveSentMessage(messageType, data, latency * latencyMultiplier);
        }

        if (sendPlayerDataWrong && messageType == MESSAGE_TYPE.STRING)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendPlayerDataWrong = false;
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

        byte[] data = resendDataMessage.Serialize(admissionTimeStamp);

        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, latency * latencyMultiplier);

        if (sendResendDataWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendResendDataWrong = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
    }

    public override void SendDisconnectClient(int id)
    {
        base.SendDisconnectClient(id);

        RemoveEntityMessage removeClientMessage = new RemoveEntityMessage(id);

        if (!clients.ContainsKey(id))
        {
            return;
        }

        byte[] data = removeClientMessage.Serialize(clients[id].timeStamp);

        //SendData(data);

        SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, latency * latencyMultiplier);

        if (sendDiconnectClientWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendDiconnectClientWrong = false;
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

        if (sendConnectRequestWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }
            
            data2[data.Length - 10] -= 1;
            sendConnectRequestWrong = false;
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

        if (sendHandShakeWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendHandShakeWrong = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }
    }
    #endregion

    #region AUX
    [SyncMethod] protected override void SendData(object data)
    {
        byte[] castedData = null;

        if (data is byte[])
        {
            castedData = data as byte[];
        }
        else
        {
            castedData = (data as ReflectionMessage).Data.ToArray();
        }

        if (IsTcpConnection)
        {
            SendToTcpServer(castedData);
        }
        else
        {
            SendToUdpServer(castedData);
        }
    }

    private float RandNum()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }
    #endregion
}
