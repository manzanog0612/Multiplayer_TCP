using System.Collections.Generic;
using System.Net;

using UnityEngine;

public class ServerNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    private UdpConnection matchMakerConnection = null;
    private UdpConnection udpConnection = null;
    private TcpServerConnection tcpServerConnection = null;

    private ServerData serverData = null;

    private Dictionary<int, double> clientsLatencies = new Dictionary<int, double>();

    private bool debug = true;

    private bool aaaaa = false;
    private bool bbbbb = false;
    private bool ccccc = false;
    private bool ddddd = false;
    private bool eeeee = false;
    private bool fffff = false;
    #endregion

    #region PUBLIC_METHODS
    public void Start(int port, int id)
    {
        ipAddress = IPAddress.Parse(MatchMaker.ip);

        NetworkManager.port = port;

        serverData = new ServerData(id, port, 0);

        if (IsTcpConnection)
        {
            StartTcpServer(ipAddress, port);
        }
        else
        {
            StartUdpServer(port);
        }
    }

    public override void Update()
    {
        base.Update();

        udpConnection?.FlushReceiveData();
        matchMakerConnection?.FlushReceiveData();
        tcpServerConnection?.FlushReceiveData();
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
            SendDisconnect(clientsIds[i]);
        }

        udpConnection?.Close();
        udpConnection = null;

        SendDisconnectMessage();
    }

    #region UDP
    public void StartUdpServer(int port)
    {
        isServer = true;

        udpConnection = new UdpConnection(port, this);

        onDefineIsServer?.Invoke(isServer);
        onStartConnection?.Invoke();

        Debug.Log("Server created on port " + port.ToString());

        if (!debug)
        {
            matchMakerConnection = new UdpConnection(IPAddress.Parse(MatchMaker.ip), MatchMaker.matchMakerPort);

            SendIsOnMessage();
        }

        Application.quitting += ShutDownUdpServer;
    }

    public void UdpBroadcast(byte[] data)
    {
        if (udpConnection == null)
        {
            return;
        }

        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                udpConnection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    public void SendToSpecificClient(byte[] data, IPEndPoint ip)
    {
        udpConnection.Send(data, ip);
    }
    #endregion

    #region TCP
    public void StartTcpServer(IPAddress ip, int port)
    {
        isServer = true;

        tcpServerConnection = new TcpServerConnection(ip, port, this);

        onDefineIsServer?.Invoke(isServer);
        onStartConnection?.Invoke();
    
        Debug.Log("Server created on port " + port.ToString());

        SendIsOnMessage();
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
    #endregion

    #region PROTECTED_METHODS   
    protected override void AddClient(IPEndPoint ip, int clientId, float realtimeSinceStartup, Vector3 position, Color color)
    {
        if (ipToId.ContainsKey((ip, realtimeSinceStartup)))
        {
            return;
        }

        int id = this.clientId;
        ipToId[(ip, realtimeSinceStartup)] = id;

        Debug.Log("Server" + " is adding client: " + id.ToString());

        clients.Add(id, new Client(ip, id, realtimeSinceStartup, new Dictionary<MESSAGE_TYPE, int>(), position, color));

        onAddNewClient?.Invoke(id, (ip.Address.Address, realtimeSinceStartup), position, color);

        serverData.amountPlayers++;
        SendDataUpdate();

        this.clientId++;
    }

    protected override void RemoveClient(int id)
    {
        base.RemoveClient(id);

        if (clients.ContainsKey(id))
        {
            return;
        }

        if (clientsLatencies.ContainsKey(id))
        {
            clientsLatencies.Remove(id);
        }

        serverData.amountPlayers--;
        SendDataUpdate();
    }

    protected override void SendData(byte[] data)
    {
        if (IsTcpConnection)
        {
            TcpBroadcast(data);
        }
        else
        {
            UdpBroadcast(data);
        }
    }

    protected override void OnReceiveGameEvent((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        base.OnReceiveGameEvent(clientConnectionData, data, messageType);

        SendData(data);

        if (messageType == MESSAGE_TYPE.STRING)
        {
            SaveSentMessage(MESSAGE_TYPE.STRING, data, GetBiggerLatency() * latencyMultiplier);
        }
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    protected override void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data) 
    {
        base.ProcessSync(clientConnectionData, data);

        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        int id = ipToId[clientConnectionData];

        UpdateClientLatency(id, CalculateLatency(data));
    }

    protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
    {
        base.ProcessEntityDisconnect(ip, data);

        if (!HandleMessageSanity(ip, data, MESSAGE_TYPE.ENTITY_DISCONECT))
        {
            return;
        }

        int clientId = new RemoveEntityMessage().Deserialize(data);

        if (!clients.ContainsKey(clientId))
        {
            return;
        }

        Debug.Log("Server is sending the signal to eliminate client: " + clientId.ToString());

        RemoveClient(clientId);
    }

    protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        base.ProcessHandShake(clientConnectionData, data);

        if (!HandleMessageSanity(clientConnectionData.ip, data, MESSAGE_TYPE.HAND_SHAKE))
        {
            return;
        }

        Debug.Log("Server is processing Handshake");

        (long ip, int port, Color color) message = new HandShakeMessage().Deserialize(data);
        AddClient(clientConnectionData.ip, clientId - 1, clientConnectionData.timeStamp, Vector3.zero, message.color);
       
        SendClientListMessage(clientConnectionData.ip);
    }
    #endregion

    #region SEND_DATA_METHODS
    protected override void SendResendDataMessage(MESSAGE_TYPE messageType, IPEndPoint ip)
    {
        base.SendResendDataMessage(messageType, ip);

        ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

        byte[] data = resendDataMessage.Serialize(-1);
        
        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, GetBiggerLatency() * latencyMultiplier);

        if (aaaaa)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 5] -= 1;
            aaaaa = false;
            if (messageType == MESSAGE_TYPE.SERVER_ON || messageType == MESSAGE_TYPE.SERVER_DATA_UPDATE)
            {
                matchMakerConnection.Send(data2);
            }
            else
            {
                SendToSpecificClient(data2, ip);
            }
        }
        else
        {
            if (messageType == MESSAGE_TYPE.SERVER_ON || messageType == MESSAGE_TYPE.SERVER_DATA_UPDATE)
            {
                matchMakerConnection.Send(data);
            }
            else
            {
                SendToSpecificClient(data, ip);
            }
        }

        //SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, GetBiggerLatency() * latencyMultiplier);
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

        SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, GetBiggerLatency() * latencyMultiplier);

        if (bbbbb)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 5] -= 1;
            bbbbb = false;
            SendData(data2);
        }
        else
        {
            SendData(data);
        }

        RemoveClient(id);
    }

    private void SendDataUpdate()
    {
        ServerDataUpdateMessage serverDataUpdateMessage = new ServerDataUpdateMessage(serverData);
        byte[] data = serverDataUpdateMessage.Serialize(-1);
        if (!debug)
        {
            //matchMakerConnection.Send(data);

            Debug.Log("Send server update data to match maker");

            SaveSentMessage(MESSAGE_TYPE.SERVER_DATA_UPDATE, data, GetBiggerLatency() * latencyMultiplier);

            if (ccccc)
            {
                byte[] data2 = new byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    data2[i] = data[i];
                }

                data2[data.Length - 5] -= 1;
                ccccc = false;
                matchMakerConnection.Send(data2);
            }
            else
            {
                matchMakerConnection.Send(data);
            }
        }
    }

    private void SendIsOnMessage()
    {
        ServerOnMessage serverOnMessage = new ServerOnMessage(port);
        byte[] data = serverOnMessage.Serialize(-1);
        if (!debug)
        {
            //matchMakerConnection.Send(data);

            Debug.Log("Send server is on message to match maker");

            SaveSentMessage(MESSAGE_TYPE.SERVER_ON, data, GetBiggerLatency() * latencyMultiplier);

            if (ddddd)
            {
                byte[] data2 = new byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    data2[i] = data[i];
                }

                data2[data.Length - 5] -= 1;
                ddddd = false;
                matchMakerConnection.Send(data2);
            }
            else
            {
                matchMakerConnection.Send(data);
            }
        }
    }

    private void SendDisconnectMessage()
    {
        RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(serverData.id);
        byte[] data = removeEntityMessage.Serialize(-1);
        if (!debug)
        {
            //matchMakerConnection.Send(data);

            Debug.Log("Send server disconnect");

            SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, GetBiggerLatency() * latencyMultiplier);
            
            if (eeeee)
            {
                byte[] data2 = new byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    data2[i] = data[i];
                }

                data2[data.Length - 5] -= 1;
                eeeee = false;
                matchMakerConnection.Send(data2);
            }
            else
            {
                matchMakerConnection.Send(data);
            }
        }
    }

    private void SendClientListMessage(IPEndPoint ip)
    {
        byte[] data = new ClientsListMessage((GetClientsList(), clientId - 1)).Serialize(-1); //admission time doesn't matter in this case because server was the originator

        //SendToSpecificClient(data, ip);

        SaveSentMessage(MESSAGE_TYPE.CLIENTS_LIST, data, GetBiggerLatency() * latencyMultiplier);

        if (fffff)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 5] -= 1;
            fffff = false;
            SendToSpecificClient(data2, ip);
        }
        else
        {
            SendToSpecificClient(data, ip);
        }
    }
    #endregion

    #region AUX
    private bool HandleMessageSanity(IPEndPoint ip, byte[] data, MESSAGE_TYPE messageType)
    {
        if (!wasLastMessageSane)
        {
            SendResendDataMessage(messageType, ip);
        }
        else
        {
            SendData(data);
            SaveSentMessage(messageType, data, GetBiggerLatency() * latencyMultiplier);
        }

        return wasLastMessageSane;
    }

    private double GetBiggerLatency()
    {
        double latency = 20;

        foreach (var clientLatency in clientsLatencies)
        {
            if (clientLatency.Value > latency)
            {
                latency = clientLatency.Value;
            }
        }

        return latency;
    }

    private void UpdateClientLatency(int id, double latency)
    {
        if (clientsLatencies.ContainsKey(id))
        {
            clientsLatencies[id] = latency;
        }
        else
        {
            clientsLatencies.Add(id, latency);
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
    #endregion
}
