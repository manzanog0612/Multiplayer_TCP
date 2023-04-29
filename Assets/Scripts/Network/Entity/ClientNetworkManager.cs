using System;
using System.Net;

using UnityEngine;

public class ClientNetworkManager : NetworkManager
{
    #region PRIVATE_FIELDS
    protected UdpConnection udpConnection = null;
    protected TcpClientConnection tcpClientConnection = null;

    private bool waitingHandShakeBack = false;
    private bool waitingConnectionRequestBack = false;

    private float handShakeTimer = 0.0f;
    private float connectionRequestTimer = 0.0f;
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

        //if (waitingHandShakeBack)
        //{
        //    WaitForAction(5, ref handShakeTimer, () =>
        //    {
        //        SendHandShake();
        //        handShakeTimer = 0;
        //    });
        //}
        //
        //if (waitingConnectionRequestBack)
        //{
        //    WaitForAction(5, ref connectionRequestTimer, () =>
        //    {
        //        SendConnectRequest();
        //        connectionRequestTimer = 0;
        //    });
        //}
    }

    public void DisconectClient()
    {
        Debug.Log("This player is going to be eliminated");
        KickClient(assignedId, true);
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
        udpConnection.Send(data);

        onSendData?.Invoke();
    }
    #endregion

    #region TCP
    public void StartTcpClient(IPAddress ip, int port)
    {
        isServer = false;

        NetworkManager.port = port;
        this.ipAddress = ip;

        tcpClientConnection = new TcpClientConnection(ip, port, this);

        //SendHandShake();
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
    protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
    {
        (long server, int port) serverData = new ConnectRequestMessage().Deserialize(data);

        udpConnection.Close();
        udpConnection = null;
        udpConnection = new UdpConnection(new IPAddress(serverData.server), serverData.port, this);

        port = serverData.port;

        Debug.Log("Received connection data, now connecting to port " + port.ToString() + " and sending handshake.");

        SendHandShake();

        onStartConnection.Invoke();
    }

    protected override void ProcessRemoveClient(byte[] data)
    {
        int clientId = new RemoveClientMessage().Deserialize(data);

        if (!clients.ContainsKey(clientId))
        {
            return;
        }

        if (clientId == assignedId)
        {
            Application.Quit();
        }
        else
        {
            RemoveClient(clientId);
        }
    }

    protected override void ProcessClientList(byte[] data)
    {
        Debug.Log("Client" + assignedId.ToString() + " is adding processing client list");

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

        Debug.Log("Client" + assignedId.ToString() + " got his id = " + id.ToString() + " assigned for first time");
        assignedId = id;

        waitingHandShakeBack = false;
    }
    #endregion

    #region PRIVATE_METHODS
    private void SendConnectRequest()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((client.Address.Address, client.Port));
        waitingConnectionRequestBack = true;

        if (IsTcpConnection)
        {
            SendToTcpServer(connectRequestMessage.Serialize(admissionTimeStamp));
        }
        else
        {
            SendToUdpServer(connectRequestMessage.Serialize(admissionTimeStamp));
        }
    }

    private void SendHandShake()
    {
        IPEndPoint client = new IPEndPoint(ipAddress, port);
        admissionTimeStamp = Time.realtimeSinceStartup;

        HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port, new Color(RandNum(), RandNum(), RandNum(), 1)));
        waitingHandShakeBack = true;

        if (IsTcpConnection)
        {
            SendToTcpServer(handShakeMessage.Serialize(admissionTimeStamp));
        }
        else
        {
            SendToUdpServer(handShakeMessage.Serialize(admissionTimeStamp));
        }
    }
    #endregion

    #region AUX
    private void WaitForAction(int maxWaitTime, ref float time, Action callback)
    {
        if (time < maxWaitTime)
        {
            time += Time.deltaTime;
        }
        else
        {
            callback.Invoke();
        }
    }

    private float RandNum()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }
    #endregion
}
