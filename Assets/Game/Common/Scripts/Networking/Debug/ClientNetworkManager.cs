using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Reflection.Attributes;
using MultiplayerLibrary.Tcp;
using System.Net;
using UnityEngine;

namespace MultiplayerLibrary.Entity
{
    public class ClientNetworkManager : NetworkManager
    {
        #region PROTECTED_FIELDS
        protected UdpConnection udpConnection = null;
        protected TcpClientConnection tcpClientConnection = null;

        protected double latency = minimunSaveTime;
        #endregion

        #region PROPERTIES
        public float admissionTimeStamp { get; private set; }
        #endregion

        #region PUBLIC_METHODS
        public double GetLatency()
        {
            return latency;
        }

        public override void Update()
        {
            base.Update();

            udpConnection?.FlushReceiveData();
            tcpClientConnection?.FlushReceiveData();
        }

        public void DisconectClient()
        {
            Debug.Log("This player is going to be eliminated");
            SendDisconnectClient(assignedId);
        }

        #region UDP
        public void StartUdpClient(IPAddress ip, int port)
        {
            isServer = false;

            NetworkManager.port = port;
            ipAddress = ip;

            udpConnection = new UdpConnection(ip, port, this);

            onDefineIsServer?.Invoke(isServer);

            Application.quitting += DisconectClient;
        }

        public void SendToUdpServer(byte[] data)
        {
            udpConnection?.Send(data);
        }
        #endregion

        #region TCP
        public void StartTcpClient(IPAddress ip, int port)
        {
            isServer = false;

            NetworkManager.port = port;
            ipAddress = ip;

            tcpClientConnection = new TcpClientConnection(ip, port, this);

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
        protected override void ProcessReflectionMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessReflectionMessage(ip, data);

            onReceiveReflectionData?.Invoke(data);
        }

        protected override void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            base.ProcessSync(clientConnectionData, data);

            latency = CalculateLatency(data);
            latency = latency < minimunSaveTime ? minimunSaveTime : latency;
        }

        protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
        {
            base.ProcessConnectRequest(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.CONNECT_REQUEST, ip);
                return;
            }

            (long server, int port, RoomData roomData) room = ConnectRequestMessage.Deserialize(data);

            udpConnection.Close();
            udpConnection = null;
            udpConnection = new UdpConnection(new IPAddress(room.server), room.port, this);

            port = room.port;

            Debug.Log("Received connection data, now connecting to port " + port.ToString() + " and sending handshake.");

            SendHandShake();
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

            if (clientId == assignedId)
            {
                udpConnection.Close();
            }
            else
            {
                RemoveClient(clientId);
            }
        }

        protected override void ProcessClientList(byte[] data)
        {
            base.ProcessClientList(data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.CLIENTS_LIST, null);
                return;
            }
        
            Debug.Log("Client" + assignedId.ToString() + " is adding processing client list");
        
            ((int id, long server, float timeSinceConection, Vector3 position, Color color)[] clientsList, int id) = ClientsListMessage.Deserialize(data);
        
            for (int i = 0; i < clientsList.Length; i++)
            {
                IPEndPoint client = new IPEndPoint(clientsList[i].server, port);
                AddClient(client, clientsList[i].id, clientsList[i].timeSinceConection, clientsList[i].position, clientsList[i].color);
            }
        
            Debug.Log("Client" + assignedId.ToString() + " got his id = " + id.ToString() + " assigned for first time");
            assignedId = id;
        
            onStartConnection?.Invoke();
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

            (long ip, int id, Color color) message = HandShakeMessage.Deserialize(data);
            AddClient(clientConnectionData.ip, message.id, clientConnectionData.timeStamp, Vector3.zero, message.color);
        }
        #endregion

        #region SEND_DATA_METHODS
        protected override void SendResendDataMessage(int messageType, IPEndPoint ip)
        {
            base.SendResendDataMessage(messageType, ip);

            ResendDataMessage resendDataMessage = new ResendDataMessage(messageType);

            byte[] data = resendDataMessage.Serialize(admissionTimeStamp);

            SaveAndSendData((int)MESSAGE_TYPE.RESEND_DATA, data);
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

            SaveAndSendData((int)MESSAGE_TYPE.ENTITY_DISCONECT, data);
        }

        public void SendConnectRequest(RoomData roomData)
        {
            IPEndPoint client = new IPEndPoint(ipAddress, port);
            ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage((client.Address.Address, client.Port, roomData));

            byte[] data = connectRequestMessage.Serialize(admissionTimeStamp);

            SaveAndSendData((int)MESSAGE_TYPE.CONNECT_REQUEST, data);
        }

        public void SendHandShake()
        {
            IPEndPoint client = new IPEndPoint(ipAddress, port);
            admissionTimeStamp = Time.realtimeSinceStartup;

            HandShakeMessage handShakeMessage = new HandShakeMessage((client.Address.Address, client.Port, new Color(RandNum(), RandNum(), RandNum(), 1)));

            byte[] data = handShakeMessage.Serialize(admissionTimeStamp);

            SaveAndSendData((int)MESSAGE_TYPE.HAND_SHAKE, data);
        }

        protected void SaveAndSendData(int messageType, byte[] data)
        {
            SaveSentMessage(messageType, data, latency * latencyMultiplier);
            SendData(data);
        }
        #endregion

        #region AUX
        [SyncMethod]
        private void SendData2(object data)
        {
            SendData(data as byte[]);
        }

        [SyncMethod]
        public override void SendData(byte[] data)
        {
            if (IsTcpConnection)
            {
                SendToTcpServer(data);
            }
            else
            {
                SendToUdpServer(data);
            }
        }

        private float RandNum()
        {
            return Random.Range(0f, 1f);
        }
        #endregion
    }
}
