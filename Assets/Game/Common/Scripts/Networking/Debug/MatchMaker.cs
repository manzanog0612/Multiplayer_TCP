using System.Diagnostics;
using System.Net;

using UnityEngine;

using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;

using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Network.Message.Constants;

namespace MultiplayerLibrary
{
    public class MatchMaker : NetworkManager, IReceiveData
    {
        #region PRIVATE_METHODS
        private UdpConnection clientUdpConnection = null;

        private int serverId = 0;

        private Dictionary<int, RoomData> rooms = new Dictionary<int, RoomData>();
        private Dictionary<int, Process> processes = new Dictionary<int, Process>();

        private IPEndPoint lastClientIp = null;
        #endregion

        #region CONSTANTS
        public const int matchMakerPort = 8053;
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
        protected override void ProcessNoticeMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessNoticeMessage(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.NOTICE, ip);
                return;
            }

            NOTICE notice = (NOTICE)NoticeMessage.Deserialize(data);

            switch (notice)
            {
                case NOTICE.ROOM_REQUEST:
                    SendRoomDatas(ip);
                    break;
                case NOTICE.FULL_ROOM:
                    break;
                default:
                    break;
            }            
        }

        protected override void ProcessResendData(IPEndPoint ip, byte[] data)
        {
            int messageTypeToResend = ResendDataMessage.Deserialize(data);
            ResendDataMessage resendDataMessage = new ResendDataMessage(messageTypeToResend);

            wasLastMessageSane = CheckMessageSanity(data, ResendDataMessage.GetHeaderSize(), ResendDataMessage.GetMessageSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage(messageTypeToResend, ip);
                return;
            }

            Debug.Log("Received the sign to resend data " + messageTypeToResend);

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
                SendResendDataMessage((int)MESSAGE_TYPE.SERVER_DATA_UPDATE, ip);
                return;
            }

            RoomData roomData = RoomDataUpdateMessage.Deserialize(data);

            if (rooms.ContainsKey(roomData.Id))
            {
                if (roomData.RoomFull || rooms[roomData.Id].InMatch)
                {
                    roomData.InMatch = true;
                }

                rooms[roomData.Id] = roomData;

                Debug.Log("Received data update from server " + roomData.Id + "- CLIENTS: " + roomData.PlayersIn);
            }
        }

        protected override void ProcessRoomOn(IPEndPoint ip, byte[] data)
        {
            base.ProcessRoomOn(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.SERVER_ON, ip);
                return;
            }

            int roomOnPort = RoomOnMessage.Deserialize(data);

            RoomData room = GetRoomByPort(roomOnPort);

            if (room != null)
            {
                SendConnectDataToClient(room);
            }
        }

        protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
        {
            base.ProcessConnectRequest(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.CONNECT_REQUEST, ip);
                return;
            }

            (long server, int port, RoomData roomData) clientData = ConnectRequestMessage.Deserialize(data);

            Debug.Log("Received connection data from port " + clientData.port + ", now looking for server to send client");

            RoomData availableRoom = GetAvailableRoom(clientData.roomData);

            lastClientIp = ip;

            if (availableRoom == null)
            {
                Debug.Log("No server was available, opening new one");
                RunNewRoom(clientData.roomData);
            }
            else
            {
                Debug.Log("Found available server");
                SendConnectDataToClient(availableRoom);
            }
        }

        protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
        {
            base.ProcessEntityDisconnect(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.ENTITY_DISCONECT, ip);
                return;
            }

            int serverId = RemoveEntityMessage.Deserialize(data);

            Debug.Log("MatchMaker received Server disconnect message for server " + serverId.ToString());

            if (rooms.ContainsKey(serverId))
            {
                rooms.Remove(serverId);

                if (!processes[serverId].HasExited)
                {
                    processes[serverId].Close();
                }

                processes.Remove(serverId);
            }
        }
        #endregion

        #region SEND_DATA_METHODS
        protected void SendRoomDatas(IPEndPoint ip)
        {
            List<RoomData> romDatas = new List<RoomData>();

            foreach (KeyValuePair<int, RoomData> room in rooms)
            {
                romDatas.Add(room.Value);
            }

            RoomsDataMessage roomsDataMessage = new RoomsDataMessage(romDatas.ToArray());
            byte[] data = roomsDataMessage.Serialize();

            OnSendData(MESSAGE_TYPE.NOTICE, data, ip);
        }

        protected override void SendResendDataMessage(int messageType, IPEndPoint ip)
        {
            base.SendResendDataMessage(messageType, ip);

            ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

            byte[] data = resendDataMessage.Serialize();

            OnSendData(MESSAGE_TYPE.RESEND_DATA, data, ip);
        }

        private void SendConnectDataToClient(RoomData roomData)
        {
            Debug.Log("Sending connection data to client");

            ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((ipAddress.Address, GetPort(roomData.Id), roomData));

            byte[] data = connectRequestMessage.Serialize();

            OnSendData(MESSAGE_TYPE.CONNECT_REQUEST, data, lastClientIp);
        }
        #endregion

        #region PRIVATE_METHODS
        private RoomData RunNewRoom(RoomData roomData)
        {
            int port = GetPort(serverId);

            string assetsPath = Application.dataPath;

#if UNITY_EDITOR
        string buildPath = assetsPath.Substring(0, assetsPath.LastIndexOf('/')) + "/Builds/Server/Multiplayer.exe";
#else
            string buildPath = assetsPath.Substring(0, assetsPath.LastIndexOf('/'));
            buildPath = buildPath.Substring(0, buildPath.LastIndexOf('/')) + "/Server/Multiplayer.exe";
#endif

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = port.ToString() + "-" + serverId.ToString() + "-" + roomData.PlayersMax.ToString() + "-" + roomData.MatchTime.ToString();
            start.FileName = buildPath;

            Process process = Process.Start(start);

            RoomData server = new RoomData(serverId, 0, roomData.PlayersMax, roomData.MatchTime);

            rooms.Add(serverId, server);
            processes.Add(serverId, process);

            serverId++;

            return server;
        }

        private RoomData GetAvailableRoom(RoomData roomData)
        {
            UpdateDictionaries();

            foreach (var server in rooms)
            {
                if (server.Value.Id == roomData.Id)
                {
                    return rooms[server.Key];
                }
            }

            return null;
        }

        private int GetPort(int roomId)
        {
            return roomId + 1 + matchMakerPort;
        }

        private RoomData GetRoomByPort(int port)
        {
            foreach (var server in rooms)
            {
                if (GetPort(server.Value.Id) == port)
                {
                    return rooms[server.Key];
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
                rooms.Remove(idsToRemove[i]);
            }
        }

        private void OnSendData(MESSAGE_TYPE messageType, byte[] data, IPEndPoint ip)
        {
            clientUdpConnection.Send(data, ip);

            double latency = CalculateLatency(data);
            SaveSentMessage(messageType, data, latency < minimunSaveTime ? minimunSaveTime : latency * latencyMultiplier);
        }
        #endregion
    }
}

