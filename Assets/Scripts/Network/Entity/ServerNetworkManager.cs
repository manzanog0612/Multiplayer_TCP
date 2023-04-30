using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    protected UdpConnection matchMakerConnection = null;
    protected UdpConnection udpConnection = null;
    protected TcpServerConnection tcpServerConnection = null;

    private ServerData serverData = null;
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
            KickClient(clientsIds[i], false);
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

        matchMakerConnection = new UdpConnection(IPAddress.Parse(MatchMaker.ip), MatchMaker.matchMakerPort);

        SendIsOnMessage();

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

        onSendData?.Invoke();
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

        serverData.amountPlayers--;
        SendDataUpdate();
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    protected override void ProcessRemoveClient(byte[] data)
    {
        int clientId = new RemoveEntityMessage().Deserialize(data);

        if (!clients.ContainsKey(clientId))
        {
            return;
        }

        Debug.Log("Server is sending the signal to eliminate client: " + clientId.ToString());

        if (IsTcpConnection)
        {
            TcpBroadcast(data);
        }
        else
        {
            UdpBroadcast(data); //admission time doesn't matter in this case because server was the originator
        }

        RemoveClient(clientId);
    }

    protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
    {
        Debug.Log("Server is processing Handshake");

        (long ip, int port, Color color) message = new HandShakeMessage().Deserialize(data);
        AddClient(clientConnectionData.ip, clientId - 1, clientConnectionData.timeStamp, Vector3.zero, message.color);
        OnReceiveEvent(clientConnectionData, data, MESSAGE_TYPE.HAND_SHAKE);

        if (IsTcpConnection)
        {
            TcpBroadcast(new ClientsListMessage((GetClientsList(), clientId - 1)).Serialize(-1));
        }
        else
        {
            UdpBroadcast(new ClientsListMessage((GetClientsList(), clientId - 1)).Serialize(-1)); //admission time doesn't matter in this case because server was the originator
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void SendDataUpdate()
    {
        ServerDataUpdateMessage serverOnMessage = new ServerDataUpdateMessage(serverData);
        matchMakerConnection.Send(serverOnMessage.Serialize(-1));

        Debug.Log("Send server update data to match maker");
    }

    private void SendIsOnMessage()
    {
        ServerOnMessage serverOnMessage = new ServerOnMessage(port);
        matchMakerConnection.Send(serverOnMessage.Serialize(-1));

        Debug.Log("Send server is on message to match maker");
    }

    private void SendDisconnectMessage()
    {
        RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(serverData.id);
        matchMakerConnection.Send(removeEntityMessage.Serialize(-1));

        Debug.Log("Send server disconnect");
    }
    #endregion

    #region AUX
    protected override void OnReceiveEvent((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
    {
        if (!ipToId.ContainsKey(clientConnectionData))
        {
            return;
        }

        base.OnReceiveEvent(clientConnectionData, data, messageType);
        
        int id = ipToId[clientConnectionData];

        onReceiveServerSyncMessage?.Invoke(id);

        if (IsTcpConnection)
        {
            TcpBroadcast(data);
        }
        else
        {
            UdpBroadcast(data);
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
