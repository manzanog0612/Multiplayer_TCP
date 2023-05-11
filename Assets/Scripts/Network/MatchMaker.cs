using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using UnityEngine;

using Debug = UnityEngine.Debug;

public class MatchMaker : NetworkManager, IReceiveData
{
    #region PRIVATE_METHODS
    private UdpConnection clientUdpConnection = null;

    private int serverId = 0;

    private Dictionary<int, ServerData> servers = new Dictionary<int, ServerData>();
    private Dictionary<int, Process> processes = new Dictionary<int, Process>();

    private IPEndPoint lastClientIp = null;

    private bool sendResendDataWrong = false;
    private bool sendConnectDataWrong = false;
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
    protected override void ProcessResendData(IPEndPoint ip, byte[] data)
    {
        MESSAGE_TYPE messageTypeToResend = new ResendDataMessage().Deserialize(data);
        ResendDataMessage resendDataMessage = new ResendDataMessage(messageTypeToResend);

        wasLastMessageSane = CheckMessageSanity(data, resendDataMessage.GetHeaderSize(), resendDataMessage.GetMessageSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.RESEND_DATA, ip);
            return;
        }

        Debug.Log("Received the sign to resend data " + (int)messageTypeToResend);

        if (lastSemiTcpMessages.ContainsKey(messageTypeToResend))
        {
            clientUdpConnection.Send(lastSemiTcpMessages[messageTypeToResend].data, ip);
        }
        else
        {
            Debug.Log("There wasn't data of message type " + (int)messageTypeToResend);
        }
    }

    protected override void ProcessServerDataUpdate(IPEndPoint ip, byte[] data)
    {
        base.ProcessServerDataUpdate(ip, data);

        if (!wasLastMessageSane)
        {
            SendResendDataMessage(MESSAGE_TYPE.SERVER_DATA_UPDATE, ip);
            return;
        }

        ServerData serverData = new ServerDataUpdateMessage().Deserialize(data);

        if (servers.ContainsKey(serverData.id))
        {
            servers[serverData.id] = serverData;

            Debug.Log("Received data update from server " + serverData.id + "- CLIENTS: " + serverData.amountPlayers);
        }
    }

    protected override void ProcessServerOn(IPEndPoint ip, byte[] data)
    {
        base.ProcessServerOn(ip, data);

        if (!wasLastMessageSane)
        {
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

    #region SEND_DATA_METHODS
    protected override void SendResendDataMessage(MESSAGE_TYPE messageType, IPEndPoint ip)
    {
        base.SendResendDataMessage(messageType, ip);

        ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

        byte[] data = resendDataMessage.Serialize(-1);

        //clientUdpConnection.Send(data, ip);

        double latency = CalculateLatency(data);

        SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, latency < minimunSaveTime ? minimunSaveTime : latency * latencyMultiplier);

        if (sendResendDataWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendResendDataWrong = false;
            clientUdpConnection.Send(data2, ip);
        }
        else
        {
            clientUdpConnection.Send(data, ip);
        }
    }

    private void SendConnectDataToClient(ServerData availableServer)
    {
        Debug.Log("Sending connection data to client");

        ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((ipAddress.Address, availableServer.port));

        byte[] data = connectRequestMessage.Serialize(-1);

        //clientUdpConnection.Send(data, lastClientIp);

        double latency = CalculateLatency(data);

        SaveSentMessage(MESSAGE_TYPE.CONNECT_REQUEST, data, latency < minimunSaveTime ? minimunSaveTime : latency * latencyMultiplier);

        if (sendConnectDataWrong)
        {
            byte[] data2 = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data2[i] = data[i];
            }

            data2[data.Length - 10] -= 1;
            sendConnectDataWrong = false;
            clientUdpConnection.Send(data2, lastClientIp);
        }
        else
        {
            clientUdpConnection.Send(data, lastClientIp);
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private ServerData RunNewServer()
    {
        int port = GetAvailablePort();

        string assetsPath = Application.dataPath;

#if UNITY_EDITOR
        string buildPath = assetsPath.Substring(0, assetsPath.LastIndexOf('/')) + "/Builds/Server/Multiplayer.exe";
#else
        string buildPath = assetsPath.Substring(0, assetsPath.LastIndexOf('/'));
        buildPath = buildPath.Substring(0, buildPath.LastIndexOf('/')) + "/Server/Multiplayer.exe";
#endif

        ProcessStartInfo start = new ProcessStartInfo();
        start.Arguments = port.ToString() + "-" + serverId.ToString();
        start.FileName = buildPath;
        
        Process process = Process.Start(start);
        
        ServerData server = new ServerData(serverId, port, 0);
        
        servers.Add(serverId, server);
        processes.Add(serverId, process);
        
        serverId++;
        
        return server;
    }
#endregion

#region AUX
    private ServerData GetAvailableServer()
    {
        UpdateDictionaries();

        foreach (var server in servers)
        {
            if (server.Value.amountPlayers < amountPlayersPerMatch)
            {
                return servers[server.Key];
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

    private void UpdateDictionaries()
    {
        List<int> idsToRemove = new List<int>();

        foreach (var process in processes)
        {
            if (process.Value.HasExited)
            {
                idsToRemove.Add(process.Key);
            }
        }

        for (int i = 0; i < idsToRemove.Count; i++)
        {
            processes.Remove(idsToRemove[i]);
            servers.Remove(idsToRemove[i]);
        }
    }
#endregion
}
