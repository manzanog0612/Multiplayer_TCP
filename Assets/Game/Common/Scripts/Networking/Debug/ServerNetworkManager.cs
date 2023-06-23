using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Parts;
using MultiplayerLibrary.Network.Message.Constants;
using MultiplayerLibrary.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace MultiplayerLibrary.Entity
{
    public class ServerNetworkManager : NetworkManager
    {
        #region PRIVATE_FIELDS
        private UdpConnection matchMakerConnection;
        private UdpConnection udpConnection;
        private TcpServerConnection tcpServerConnection;

        private RoomData roomData;

        private Dictionary<int, double> clientsLatencies = new Dictionary<int, double>();

        private bool debug = false;

        private bool matchStarted = false;
        private float matchTime = 0;
        #endregion

        #region PUBLIC_METHODS
        public void Start(int port, int id, int playersMax, int matchTime)
        {
            ipAddress = IPAddress.Parse(MatchMaker.ip);

            NetworkManager.port = port;

            roomData = new RoomData(id, 0, playersMax, matchTime);
            this.matchTime = matchTime;

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

            if (matchStarted)
            {
                matchTime -= Time.deltaTime;
                SendTimerUpdateMessage();
            }
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
                SendDisconnectClient(clientsIds[i]);
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
                matchMakerConnection = new UdpConnection(IPAddress.Parse(MatchMaker.ip), MatchMaker.matchMakerPort, this);

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

            roomData.PlayersIn++;
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

            roomData.PlayersIn--;
            SendDataUpdate();
        }

        public override void SendData(byte[] data)
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
        #region DEBUG
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

        protected void SendRoomDatas(IPEndPoint ip)
        {
            List<RoomData> romDatas = new List<RoomData> { roomData};

            RoomsDataMessage roomsDataMessage = new RoomsDataMessage(romDatas.ToArray());
            byte[] data = roomsDataMessage.Serialize();

            SaveSentMessage(MESSAGE_TYPE.NOTICE, data, GetBiggerLatency() * latencyMultiplier);

            SendToSpecificClient(data, ip);
        }
        #endregion

        protected override void ProcessGameMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessGameMessage(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.GAME_MESSAGE, ip);
                return;
            }

            SendData(data);

            SaveSentMessage(MESSAGE_TYPE.GAME_MESSAGE, data, GetBiggerLatency() * latencyMultiplier);
        }

        protected override void ProcessReflectionMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessReflectionMessage(ip, data);

            int clientId = BitConverter.ToInt32(data, sizeof(bool));

            if (clients.ContainsKey(clientId))
            {
                SendData(data);
            }
        }

        protected override void ProcessResendData(IPEndPoint ip, byte[] data)
        {
            int messageTypeToResend = ResendDataMessage.Deserialize(data);
            ResendDataMessage resendDataMessage = new ResendDataMessage(messageTypeToResend);

            wasLastMessageSane = CheckMessageSanity(data, ResendDataMessage.GetHeaderSize(), ResendDataMessage.GetMessageSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.RESEND_DATA, ip);
                return;
            }

            Debug.Log("Received the sign to resend data " + (int)messageTypeToResend);

            if (lastSemiTcpMessages.ContainsKey(messageTypeToResend))
            {
                if (messageTypeToResend == (int)MESSAGE_TYPE.SERVER_ON || messageTypeToResend == (int)MESSAGE_TYPE.SERVER_DATA_UPDATE)
                {
                    matchMakerConnection.Send(lastSemiTcpMessages[messageTypeToResend].data);
                }
                else
                {
                    SendData(lastSemiTcpMessages[messageTypeToResend].data);
                }
            }
            else
            {
                Debug.Log("There wasn't data of message type " + (int)messageTypeToResend);
            }
        }

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

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.ENTITY_DISCONECT, ip);
                return;
            }

            int clientId = RemoveEntityMessage.Deserialize(data);

            if (!clients.ContainsKey(clientId))
            {
                return;
            }

            Debug.Log("Server is sending the signal to eliminate client: " + clientId.ToString());

            SendDisconnectClient(clientId);
        }

        protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            base.ProcessHandShake(clientConnectionData, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.HAND_SHAKE, clientConnectionData.ip);
                return;
            }

            Debug.Log("Server is processing Handshake");

            (long ip, int port, Color color) message = HandShakeMessage.Deserialize(data);

            SendHandShakeForClients(message, clientConnectionData.timeStamp);

            AddClient(clientConnectionData.ip, clientId, clientConnectionData.timeStamp, Vector3.zero, message.color);

            SendClientListMessage(clientConnectionData.ip);

            if (clients.Count == roomData.PlayersMax)
            {
                SendRoomFullNotice();
            }
        }
        #endregion

        #region SEND_DATA_METHODS
        private void SendTimerUpdateMessage()
        {
            TimerMessage timerMessage = new TimerMessage(matchTime);

            byte[] data = timerMessage.Serialize();
            SendData(data);
        }

        private void SendHandShakeForClients((long ip, int port, Color color) message, float connectionTime)
        {
            HandShakeMessage handShakeMessageForClients = new HandShakeMessage((message.ip, clientId, message.color));

            byte[] data = handShakeMessageForClients.Serialize(connectionTime);

            SendData(data);
            SaveSentMessage(MESSAGE_TYPE.HAND_SHAKE, data, GetBiggerLatency() * latencyMultiplier);
        }

        protected override void SendResendDataMessage(int messageType, IPEndPoint ip)
        {
            base.SendResendDataMessage(messageType, ip);

            ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

            byte[] data = resendDataMessage.Serialize();

            SaveSentMessage(MESSAGE_TYPE.RESEND_DATA, data, GetBiggerLatency() * latencyMultiplier);

            SendToSpecificClient(data, ip);
        }

        public override void SendDisconnectClient(int id)
        {
            base.SendDisconnectClient(id);

            RemoveEntityMessage removeClientMessage = new RemoveEntityMessage(id);

            if (!clients.ContainsKey(id))
            {
                return;
            }

            byte[] data = removeClientMessage.Serialize(clients[id].timeStamp);

            SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, GetBiggerLatency() * latencyMultiplier);

            SendData(data);

            RemoveClient(id);
        }

        private void SendDataUpdate()
        {
            //RoomDataUpdateMessage roomDataUpdateMessage = new RoomDataUpdateMessage(roomData);
            //byte[] data = roomDataUpdateMessage.Serialize();
            //
            //RoomData roomDataa = Deserialize2(data, RoomDataUpdateMessage.GetHeaderSize());
            //
            //RoomDataUpdateMessage serverDataUpdateMessage = new RoomDataUpdateMessage(roomDataa);
            //byte[] data2 = serverDataUpdateMessage.Serialize();
            //
            //HandleMessageError2(data, (int)MESSAGE_TYPE.SERVER_DATA_UPDATE, serverDataUpdateMessage, RoomDataUpdateMessage.GetMessageSize(), RoomDataUpdateMessage.GetHeaderSize());

            RoomDataUpdateMessage roomDataUpdateMessage = new RoomDataUpdateMessage(roomData);
            byte[] data = roomDataUpdateMessage.Serialize();
            
            if (!debug)
            {
                Debug.Log("Send server update data to match maker");
            
                SaveSentMessage(MESSAGE_TYPE.SERVER_DATA_UPDATE, data, GetBiggerLatency() * latencyMultiplier);
            
                matchMakerConnection.Send(data);
            }
        }

        #region DEBUG
        public static RoomData Deserialize2(byte[] message, int headerSize)
        {
            return new RoomData(BitConverter.ToInt32(message, headerSize), 
                BitConverter.ToInt32(message, headerSize + 4), 
                BitConverter.ToInt32(message, headerSize + 8), 
                BitConverter.ToInt32(message, headerSize + 12));
        }

        protected virtual void HandleMessageError2(byte[] data, int type, SemiTcpMessage semiTcpMessage, int messageSize, int headerSize)
        {
            wasLastMessageSane = CheckMessageSanity2(data, headerSize, messageSize, semiTcpMessage.GetMessageTail().messageOperationResult);
            if (!wasLastMessageSane)
            {
                Debug.Log("The message " + type + " was insane");
            }
        }

        protected bool CheckMessageSanity2(byte[] data, int headerSize, int messageSize, int op)
        {
            MessageTail messageTail = DeserializeTail2(data, headerSize, messageSize);
            if (op == messageTail.messageOperationResult)
            {
                return data.Length == messageTail.messageSize;
            }

            return false;
        }

        public MessageTail DeserializeTail2(byte[] message, int headerSize, int messageSize)
        {
            return new MessageTail(BitConverter.ToInt32(message, headerSize + messageSize), BitConverter.ToInt32(message, headerSize + messageSize + 4));
        }
        #endregion

        private void SendIsOnMessage()
        {
            RoomOnMessage serverOnMessage = new RoomOnMessage(port);
            byte[] data = serverOnMessage.Serialize();
            if (!debug)
            {
                Debug.Log("Send server is on message to match maker");

                SaveSentMessage(MESSAGE_TYPE.SERVER_ON, data, GetBiggerLatency() * latencyMultiplier);

                matchMakerConnection.Send(data);
            }
        }

        private void SendDisconnectMessage()
        {
            RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(roomData.Id);
            byte[] data = removeEntityMessage.Serialize();

            if (!debug)
            {
                Debug.Log("Send server disconnect");

                SaveSentMessage(MESSAGE_TYPE.ENTITY_DISCONECT, data, GetBiggerLatency() * latencyMultiplier);

                matchMakerConnection.Send(data);
            }
        }

        private void SendClientListMessage(IPEndPoint ip)
        {
            ClientsListMessage clientsListMessage = new ClientsListMessage((GetClientsList(), clientId - 1));
            byte[] data = clientsListMessage.Serialize();

            SaveSentMessage(MESSAGE_TYPE.CLIENTS_LIST, data, GetBiggerLatency() * latencyMultiplier);
            SendToSpecificClient(data, ip);
        }

        private void SendRoomFullNotice()
        {
            NoticeMessage noticeMessage = new NoticeMessage((int)NOTICE.FULL_ROOM);
            byte[] data = noticeMessage.Serialize();

            SaveSentMessage(MESSAGE_TYPE.NOTICE, data, GetBiggerLatency() * latencyMultiplier);
            SendData(data);

            matchStarted = true;
        }
        #endregion

        #region AUX
        private double GetBiggerLatency()
        {
            double latency = 0.01f;

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
}
