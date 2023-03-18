using System;
using System.Collections.Generic;
using System.Net;

using UnityEngine;

public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public bool tcpConnection = true;
    public Logger logger = null;

    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection udpConnection;
    private TcpClientConnection tcpClientConnection;
    private TcpServerConnection tcpServerConnection;

    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    int clientId = 0; // This id should be generated during first handshake

    #region UDP
    public void StartUdpServer(int port)
    {
        isServer = true;
        this.port = port;
        udpConnection = new UdpConnection(port, this);
    }

    public void StartUdpClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        udpConnection = new UdpConnection(ip, port, this);

        AddClient(new IPEndPoint(ip, port));
    }

    public void OnReceiveDataUdp(byte[] data, IPEndPoint ip)
    {
        AddClient(ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
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
    public void StartTcpServer(string server, int port)
    {
        isServer = true;
        this.port = port;
        tcpServerConnection = new TcpServerConnection(server, port, this, logger);
    }

    public void StartTcpClient(IPAddress ip, string server, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        tcpClientConnection = new TcpClientConnection(server, port, this, logger);

        AddClient(new IPEndPoint(ip, port));
    }

    public void OnReceiveDataTcp(byte[] data, IPEndPoint ip)
    {
        logger.SendLog("RECEIVED DATA");

        AddClient(ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToTcpServer(byte[] data)
    {
        tcpClientConnection.Send(data);
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

    private void Update()
    {
        // Flush the data in main thread
        if (tcpClientConnection != null)
            tcpClientConnection.FlushReceiveData();

        if (tcpServerConnection != null)
            tcpServerConnection.FlushReceiveData();

        if (udpConnection != null)
            udpConnection.FlushReceiveData();
    }

    private void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            logger.SendLog("Adding client: " + ip.Address);

            int id = clientId;
            ipToId[ip] = clientId;

            clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup));

            clientId++;
        }
    }

    private void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            logger.SendLog("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }
}
