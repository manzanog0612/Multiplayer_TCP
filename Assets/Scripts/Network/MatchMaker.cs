using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

using UnityEngine;
using Debug = UnityEngine.Debug;

public class MatchMaker : MonoBehaviour, IReceiveData
{
    #region PRIVATE_METHODS
    private UdpConnection clientUdpConnection = null;
    private UdpConnection serverUdpConnection = null;

    private IPAddress ipAddress = null;
    private int port = 0;

    private int serverId = 0;

    private List<ServerData> servers = new List<ServerData>();
    private Dictionary<int, Process> processes = new Dictionary<int, Process>();
    #endregion

    #region CONSTANTS
    private const int startingPort = 8054;
    private const int amountPlayersPerMatch = 2;
    #endregion

    #region UNITY_CALLS
#if UNITY_SERVER
    private void Start()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8053;

        //if (tcpConnection)
        //{
        //    StartTcpServer(ipAddress, port);
        //}
        //else
        //{
            StartUdpServer(port);
        //}
    }
#endif

    private void Update()
    {
        clientUdpConnection?.FlushReceiveData();
        serverUdpConnection?.FlushReceiveData();
    }
    #endregion

    #region PUBLIC_METHODS
    public void StartUdpServer(int port)
    {
        this.port = port;
        clientUdpConnection = new UdpConnection(port, this);

        Debug.Log("Server created");
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);

        switch (messageType)
        {
            case MESSAGE_TYPE.CONNECT_REQUEST:
                ProcessConnectRequest(ip, data);
                //case MESSAGE_TYPE.SERVER_DATA_UPDATE
                break;
            default:
                break;
        }
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    private void ProcessConnectRequest(IPEndPoint ip, byte[] data)
    {
        ServerData availableServer = GetAvailableServer();

        if (availableServer == null)
        {
            availableServer = RunNewServer();
        }

        SendConnectRequestToServer(availableServer, data);
    }

    private void SendConnectRequestToServer(ServerData availableServer, byte[] data)
    {
        serverUdpConnection = new UdpConnection(availableServer.port, this);

        serverUdpConnection.Send(data);
    }

    private ServerData RunNewServer()
    {
        int port = GetAvailablePort();

        ProcessStartInfo start = new ProcessStartInfo();
        start.Arguments = port.ToString();
        start.FileName = "C:\\Users\\guill\\Desktop\\server\\Multiplayer.exe";
        
        Process process = Process.Start(start);

        ServerData server = new ServerData(serverId, port, 0);

        servers.Add(server);
        processes.Add(serverId, process);
        
        serverId++;

        return server;

        //using (proc)
        //{
        //    proc.WaitForExit();
        //
        //    // Retrieve the app's exit code
        //    exitCode = proc.ExitCode;
        //
        //    int closedServerId = 0;
        //    foreach (var process in processes)
        //    {
        //        if (process.Value == proc)
        //        {
        //            closedServerId = process.Key;
        //            break;
        //        }
        //    }
        //
        //    for (int i = 0; i < servers.Count; i++)
        //    {
        //        if (servers[i].id == closedServerId)
        //        {
        //            servers.Remove(servers[i]);
        //            break;
        //        }
        //    }
        //    
        //}
    }

    private ServerData GetAvailableServer()
    {
        for (int i = 0; i < servers.Count; i++)
        {
            if (servers[i].amountPlayers < amountPlayersPerMatch)
            {
                return servers[i];
            }
        }

        return null;
    }

    private int GetAvailablePort()
    {
        int port = startingPort;
        bool portIsUsed = servers.Where(server => server.port == port).ToList().Count > 0;

        while (servers.Where(server => server.port == port).ToList().Count > 0)
        {
            port++;
        }

        return port;
    }
    #endregion
}
