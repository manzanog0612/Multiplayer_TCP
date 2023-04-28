using System;
using System.Collections;
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

    private int serverId = 0;

    private List<ServerData> servers = new List<ServerData>();
    private Dictionary<int, Process> processes = new Dictionary<int, Process>();

    private IPEndPoint lastClientIp = null;
    #endregion

    #region CONSTANTS
    public const int matchMakerPort = 8053;
    public const int startingPort = 8054;
    public const int amountPlayersPerMatch = 2;
    public const string ip = "127.0.0.1";
    #endregion

    #region UNITY_CALLS
#if UNITY_SERVER
    private void Start()
    {
        ipAddress = IPAddress.Parse(ip);

        //if (tcpConnection)
        //{
        //    StartTcpServer(ipAddress, port);
        //}
        //else
        //{
            StartUdpServer(matchMakerPort);
        //}
    }
#endif

    private void Update()
    {
        clientUdpConnection?.FlushReceiveData();
        serverUdpConnection?.FlushReceiveData();
    }

    private void OnDestroy()
    {
        clientUdpConnection?.Close();
        serverUdpConnection?.Close();
    }
    #endregion

    #region PUBLIC_METHODS
    public void StartUdpServer(int port)
    {
        clientUdpConnection = new UdpConnection(port, this);

        Debug.Log("Server created");
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);

        switch (messageType)
        {
            case MESSAGE_TYPE.SERVER_ON:
                ProcessServerOn(data);
                break;
            case MESSAGE_TYPE.CONNECT_REQUEST:
                ProcessConnectRequest(data, ip);
                //case MESSAGE_TYPE.SERVER_DATA_UPDATE
                break;
            default:
                break;
        }
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    private void ProcessServerOn(byte[] data)
    {
        int serverOnPort = new ServerOnMessage().Deserialize(data);

        SendConnectDataToClient(servers.Where(server => server.port == serverOnPort).ToList()[0]);
    }

    private void ProcessConnectRequest(byte[] data, IPEndPoint ip)
    {
        (long server, int port) clientData = new ConnectRequestMessage().Deserialize(data); // only for log

        Debug.Log("Received connection data from port " + clientData.port + ", now looking for server to send client");

        ServerData availableServer = GetAvailableServer();

        if (availableServer == null)
        {
            Debug.Log("No server was available, opening new one");
            RunNewServer();

            lastClientIp = ip;
        }
        else
        {
            Debug.Log("Found available server");
            SendConnectDataToClient(availableServer);
        }        
    }
    #endregion

    #region PRIVATE_METHODS
    private void SendConnectDataToClient(ServerData availableServer)
    {
        Debug.Log("Sending connection data to client");

        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((ipAddress.Address, availableServer.port));

        clientUdpConnection.Send(connectRequestMessage.Serialize(-1), lastClientIp);

        clientUdpConnection.Close();

        clientUdpConnection = null;
    }

    private ServerData RunNewServer()
    {
        int port = GetAvailablePort();

        ProcessStartInfo start = new ProcessStartInfo();
        start.Arguments = port.ToString();
        start.FileName = "C:\\Users\\guill\\Desktop\\server\\Multiplayer.exe";//application.datapath

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
