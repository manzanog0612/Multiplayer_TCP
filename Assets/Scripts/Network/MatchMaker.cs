using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Debug = UnityEngine.Debug;

public class MatchMaker : NetworkManager, IReceiveData
{
    #region PRIVATE_METHODS
    private UdpConnection clientUdpConnection = null;

    private int serverId = 0;

    private Dictionary<int, ServerData> servers = new Dictionary<int, ServerData>();
    private Dictionary<int, Process> processes = new Dictionary<int, Process>();

    private IPEndPoint lastClientIp = null;
    private IPEndPoint lastIp = null;


    #endregion

    #region CONSTANTS
    public const int matchMakerPort = 8053;
    public const int amountPlayersPerMatch = 2;
    public const string ip = "127.0.0.1";
    #endregion

    #region PUBLIC_METHODS
    public void Start()
    {
        ipAddress = IPAddress.Parse(ip);

        StartUdpServer(matchMakerPort);
    }

    public override void Update()
    {
        base.Update();

        clientUdpConnection?.FlushReceiveData();
    }

    public void OnDestroy()
    {
        clientUdpConnection?.Close();
    }

    public void StartUdpServer(int port)
    {
        clientUdpConnection = new UdpConnection(port, this);

        Debug.Log("Server created");
    }
    #endregion

    #region DATA_RECEIVE_PROCESS
    protected override void ProcessServerDataUpdate(IPEndPoint ip, byte[] data)
    {
        base.ProcessServerDataUpdate(ip, data);

        if (!wasLastMessageSane)
        {
            lastIp = ip;
            SendResendDataMessage(MESSAGE_TYPE.SERVER_DATA_UPDATE, ip);
            return;
        }

        ServerData serverData = new ServerDataUpdateMessage().Deserialize(data);

        if (servers.ContainsKey(serverData.id))
        {
            servers[serverData.id] = serverData;
        }
    }

    protected override void ProcessServerOn(IPEndPoint ip, byte[] data)
    {
        base.ProcessServerOn(ip, data);

        if (!wasLastMessageSane)
        {
            lastIp = ip;
            SendResendDataMessage(MESSAGE_TYPE.SERVER_ON, ip);
            return;
        }

        int serverOnPort = new ServerOnMessage().Deserialize(data);

        ServerData server = GetServerByPort(serverOnPort);

        if (server != null)
        { 
            SendConnectDataToClient(server); 
        }
    }

    protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
    {
        base.ProcessConnectRequest(ip, data);

        if (!wasLastMessageSane)
        {
            lastIp = ip;
            SendResendDataMessage(MESSAGE_TYPE.CONNECT_REQUEST, ip);
            return;
        }

        (long server, int port) clientData = new ConnectRequestMessage().Deserialize(data); // only for log and security check

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

    protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
    {
        base.ProcessEntityDisconnect(ip, data);

        if (!wasLastMessageSane)
        {
            lastIp = ip;
            SendResendDataMessage(MESSAGE_TYPE.ENTITY_DISCONECT, ip);
            return;
        }

        int serverId = new RemoveEntityMessage().Deserialize(data);

        Debug.Log("MatchMaker received Server disconnect message for server " + serverId.ToString());

        if (servers.ContainsKey(serverId))
        {
            servers.Remove(serverId);

            if (!processes[serverId].HasExited)
            { 
                processes[serverId].Close(); 
            }

            processes.Remove(serverId);
        }
    }
    #endregion

    #region PROTECTED_METHODS
    protected override void SendData(byte[] data)
    {
        clientUdpConnection.Send(data, lastIp);
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
    }

    #region AUX
    private ServerData GetAvailableServer()
    {
        foreach (var server in servers)
        {
            if (server.Value.amountPlayers < amountPlayersPerMatch)
            {
                if (processes[server.Value.id].HasExited)
                {
                    processes.Remove(server.Value.id);
                }
                else
                {
                    return servers[server.Key];
                }
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
    #endregion
}
