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

    private Dictionary<int, ServerData> servers = new Dictionary<int, ServerData>();
    private Dictionary<int, Process> processes = new Dictionary<int, Process>();

    private IPEndPoint lastClientIp = null;
    #endregion

    #region CONSTANTS
    public const int matchMakerPort = 8053;
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
            case MESSAGE_TYPE.SERVER_DATA_UPDATE:
                ProcessServerDataUpdate(data);
                break;
            case MESSAGE_TYPE.SERVER_ON:
                ProcessServerOn(data);
                break;
            case MESSAGE_TYPE.CONNECT_REQUEST:
                ProcessConnectRequest(data, ip);
                break;
            default:
                break;
        }
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    private void ProcessServerDataUpdate(byte[] data)
    {
        ServerData serverData = new ServerDataUpdateMessage().Deserialize(data);

        if (servers.ContainsKey(serverData.id))            
        {
            servers[serverData.id] = serverData;
        }
    }

    private void ProcessServerOn(byte[] data)
    {
        int serverOnPort = new ServerOnMessage().Deserialize(data);

        ServerData server = GetServerByPort(serverOnPort);

        if (server != null)
        { 
            SendConnectDataToClient(server); 
        }
    }

    private void ProcessConnectRequest(byte[] data, IPEndPoint ip)
    {
        (long server, int port) clientData = new ConnectRequestMessage().Deserialize(data); // only for log

        Debug.Log("Received connection data from port " + clientData.port + ", now looking for server to send client");

        ServerData availableServer = GetAvailableServer();

        lastClientIp = ip;

        if (availableServer == null)
        {
            Debug.Log("No server was available, opening new one");
            RunNewServer();
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
    }

    private ServerData RunNewServer()
    {
        int port = GetAvailablePort();

        ProcessStartInfo start = new ProcessStartInfo();
        start.Arguments = port.ToString() + "-" + serverId.ToString();
        start.FileName = "C:\\Users\\guill\\Desktop\\server\\Multiplayer.exe";//application.datapath

        Process process = Process.Start(start);

        ServerData server = new ServerData(serverId, port, 0);

        servers.Add(serverId, server);
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
        int port = matchMakerPort;
        bool portIsUsed = true;

        while (portIsUsed)
        {
            port++;
            portIsUsed = GetServerByPort(port) != null;
        }

        return port;
    }
    #endregion

    #region AUX
    private ServerData GetServerByPort(int port)
    {
        foreach (var server in servers)
        {
            if (server.Value.port == port)
            {
                return servers[server.Key];
            }
        }

        return null;
    }
    #endregion
}
