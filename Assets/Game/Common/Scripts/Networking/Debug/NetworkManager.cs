using System.Net;

using UnityEngine;

using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message.Formater;
using MultiplayerLibrary.Message.Parts;
using MultiplayerLibrary.Message;
using System.Collections.Generic;
using System;
using MultiplayerLib2.Network.Message;

namespace MultiplayerLibrary.Entity
{
    public class NetworkManager : IReceiveData
    {
        #region INSTANCE
        public static NetworkManager Instance = null;
        #endregion

        #region PROTECTED_FIELDS   
        protected int clientId = 0;

        protected Dictionary<int, int> lastMessagesIds = new Dictionary<int, int>();
        protected Dictionary<int, (byte[] data, float time)> lastSemiTcpMessages = new Dictionary<int, (byte[], float)>();

        protected bool wasLastMessageSane = true;

        protected Dictionary<int, Client> clients = new Dictionary<int, Client>();
        protected readonly Dictionary<(IPEndPoint, float), int> ipToId = new Dictionary<(IPEndPoint, float), int>();
        #endregion

        #region PROPERTIES
        public IPAddress ipAddress { get; protected set; }
        public static int port { get; set; }
        public bool isServer { get; protected set; }
        public bool IsTcpConnection => isTcpConnection;
        public int assignedId { get; protected set; }
        #endregion

        #region ACTIONS
        public Action<byte[], IPEndPoint, int, MESSAGE_TYPE> onReceiveEvent = null;
        public Action onStartConnection = null;
        public Action<bool> onDefineIsServer = null;
        public Action<int, (long, float), Vector3, Color> onAddNewClient = null;
        public Action<int> onRemoveClient = null;
        public Action<int> onSync = null;
        public Action<byte[]> onReceiveReflectionData = null;
        #endregion

        #region CONSTANTS
        private const bool isTcpConnection = false;
        protected const double latencyMultiplier = 5;
        protected const float minimunSaveTime = 0.1f;
        #endregion

        #region PUBLIC_METHODS
        public virtual void Update()
        {
            UpdateSavedMessagesTimes();
        }

        public virtual void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            bool isReflectionMessage = MessageFormater.IsReflectionMessage(data);

            if (isReflectionMessage)
            {
                ProcessReflectionMessage(ip, data);
            }
            else
            {
                MESSAGE_TYPE messageType = MessageFormater.GetMessageType(data);
                float timeStamp = MessageFormater.GetAdmissionTime(data);

                switch (messageType)
                {
                    case MESSAGE_TYPE.SYNC:
                        ProcessSync((ip, timeStamp), data);
                        break;
                    case MESSAGE_TYPE.RESEND_DATA:
                        ProcessResendData(ip, data);
                        break;
                    case MESSAGE_TYPE.SERVER_DATA_UPDATE:
                        ProcessServerDataUpdate(ip, data);
                        break;
                    case MESSAGE_TYPE.SERVER_ON:
                        ProcessRoomOn(ip, data);
                        break;
                    case MESSAGE_TYPE.CONNECT_REQUEST:
                        ProcessConnectRequest(ip, data);
                        break;
                    case MESSAGE_TYPE.ENTITY_DISCONECT:
                        ProcessEntityDisconnect(ip, data);
                        break;
                    case MESSAGE_TYPE.HAND_SHAKE:
                        ProcessHandShake((ip, timeStamp), data);
                        break;
                    case MESSAGE_TYPE.CLIENTS_LIST:
                        ProcessClientList(data);
                        break;
                    case MESSAGE_TYPE.STRING:
                        ProcessChatMessage((ip, timeStamp), data);
                        break;
                    case MESSAGE_TYPE.ROOM_DATAS:
                        ProcessRoomDatasMessage(ip, data);
                        break;
                    case MESSAGE_TYPE.NOTICE:
                        ProcessNoticeMessage(ip, data);
                        break;
                    case MESSAGE_TYPE.GAME_MESSAGE:
                        ProcessGameMessage(ip, data);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void SendDisconnectClient(int id)
        {
            Debug.Log("Disconecting player: " + id.ToString());
        }
        #endregion

        #region PROTECTED_METHODS

        protected virtual void AddClient(IPEndPoint ip, int clientId, float realtimeSinceStartup, Vector3 position, Color color)
        {
            if (ipToId.ContainsKey((ip, realtimeSinceStartup)))
            {
                return;
            }

            ipToId[(ip, realtimeSinceStartup)] = clientId;

            clients.Add(clientId, new Client(ip, clientId, realtimeSinceStartup, new Dictionary<MESSAGE_TYPE, int>(), position, color));

            onAddNewClient?.Invoke(clientId, (ip.Address.Address, realtimeSinceStartup), position, color);
        }

        protected virtual void RemoveClient(int id)
        {
            if (!clients.ContainsKey(id))
            {
                return;
            }

            onRemoveClient?.Invoke(id);

            Debug.Log("Removing client: " + id);
            ipToId.Remove((clients[id].ipEndPoint, clients[id].timeStamp));
            clients.Remove(id);
        }

        protected double CalculateLatency(byte[] data)
        {
            (int day, int hour, int minute, int second, int millisecond) sendTime = MessageFormater.GetMessageSendTime(data);

            DateTime utcNow = new DateTime(2023, 5, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
            DateTime messageNow = new DateTime(2023, 5, sendTime.day, sendTime.hour, sendTime.minute, sendTime.second, sendTime.millisecond);

            double latency = (utcNow - messageNow).TotalSeconds;

            //Debug.Log("L: " + latency.ToString());

            return latency;
        }

        protected bool CheckMessageSanity(byte[] data, int headerSize, int messageSize, SemiTcpMessage semiTcpMessage, int op)
        {
            MessageTail messageTail = semiTcpMessage.DeserializeTail(data, headerSize, messageSize);

            return op == messageTail.messageOperationResult && data.Length == messageTail.messageSize;
        }

        protected void SaveSentMessage(int messageType, byte[] data, double saveTime)
        {
            float millisecond = (float)saveTime;
            float saveTimeFinal = millisecond < Time.deltaTime * 2 ? Time.deltaTime * 2 : millisecond;

            if (lastSemiTcpMessages.ContainsKey(messageType))
            {
                lastSemiTcpMessages[messageType] = (data, saveTimeFinal);
            }
            else
            {
                lastSemiTcpMessages.Add(messageType, (data, saveTimeFinal));
            }

            //Debug.Log("Saved message " + (int)messageType + " for " + saveTimeFinal + " seconds");
        }

        protected void CheckIfMessageIdIsCorrect(byte[] data, int messageType)
        {
            int messageId = BitConverter.ToInt32(data, sizeof(int));

            if (lastMessagesIds.ContainsKey(messageType))
            {
                if (lastMessagesIds[messageType] <= messageId)
                {
                    lastMessagesIds[messageType] = messageId;
                }
                else
                {
                    wasLastMessageSane = false;
                }
            }
            else
            {
                lastMessagesIds.Add(messageType, messageId);
            }
        }

        protected virtual void SendResendDataMessage(int messageType, IPEndPoint ip)
        {
            Debug.Log("RESENDING DATA " + (int)messageType);
        }

        public virtual void SendData(byte[] data)
        {

        }
        #endregion

        #region DATA_RECEIVE_PROCESS
        private void ThrowInsaneMessageLogIfInsane(int message)
        {
            if (!wasLastMessageSane)
            {
                Debug.Log("The message " + message + " was insane");
            }
        }

        protected virtual void ProcessReflectionMessage(IPEndPoint ip, byte[] data)
        {

        }

        protected virtual void ProcessSync((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            if (isServer)
            {
                if (!ipToId.ContainsKey(clientConnectionData))
                {
                    return;
                }
            }

            int id = isServer ? ipToId[clientConnectionData] : -1;

            onSync?.Invoke(id);
        }

        protected virtual void ProcessResendData(IPEndPoint ip, byte[] data)
        {
            int messageTypeToResend = ResendDataMessage.Deserialize(data);
            ResendDataMessage resendDataMessage = new ResendDataMessage(messageTypeToResend);

            wasLastMessageSane = CheckMessageSanity(data, ResendDataMessage.GetHeaderSize(), ResendDataMessage.GetMessageSize(), resendDataMessage, resendDataMessage.GetMessageTail().messageOperationResult);
            ThrowInsaneMessageLogIfInsane((int)MESSAGE_TYPE.RESEND_DATA);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.RESEND_DATA, ip);
                return;
            }

            Debug.Log("Received the sign to resend data " + messageTypeToResend);

            if (lastSemiTcpMessages.ContainsKey(messageTypeToResend))
            {
                SendData(lastSemiTcpMessages[messageTypeToResend].data);
            }
            else
            {
                Debug.Log("There wasn't data of message type " + messageTypeToResend);
            }
        }

        protected virtual void ProcessServerDataUpdate(IPEndPoint ip, byte[] data)
        {
            RoomDataUpdateMessage serverDataUpdateMessage = new RoomDataUpdateMessage(RoomDataUpdateMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.SERVER_DATA_UPDATE, serverDataUpdateMessage, RoomDataUpdateMessage.GetMessageSize(), RoomDataUpdateMessage.GetHeaderSize());
        }

        protected virtual void ProcessRoomOn(IPEndPoint ip, byte[] data)
        {
            RoomOnMessage serverOnMessage = new RoomOnMessage(RoomOnMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.SERVER_ON, serverOnMessage, RoomOnMessage.GetMessageSize(), RoomOnMessage.GetHeaderSize());
        }

        protected virtual void ProcessConnectRequest(IPEndPoint ip, byte[] data)
        {
            ConnectRequestMessage connectRequestMessage = new ConnectRequestMessage(ConnectRequestMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.CONNECT_REQUEST, connectRequestMessage, ConnectRequestMessage.GetMessageSize(), ConnectRequestMessage.GetHeaderSize());
        }

        protected virtual void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
        {
            RemoveEntityMessage removeEntityMessage = new RemoveEntityMessage(RemoveEntityMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.ENTITY_DISCONECT, removeEntityMessage, RemoveEntityMessage.GetMessageSize(), RemoveEntityMessage.GetHeaderSize());
        }

        protected virtual void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            HandShakeMessage handShakeMessage = new HandShakeMessage(HandShakeMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.HAND_SHAKE, handShakeMessage, HandShakeMessage.GetMessageSize(), HandShakeMessage.GetHeaderSize());
        }

        protected virtual void ProcessClientList(byte[] data)
        {
            ClientsListMessage clientsListMessage = new ClientsListMessage(ClientsListMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.CLIENTS_LIST, clientsListMessage, clientsListMessage.GetMessageSize(), ClientsListMessage.GetHeaderSize());
        }

        protected virtual void ProcessRoomDatasMessage(IPEndPoint ip, byte[] data)
        {
            RoomsDataMessage roomDatasRequest = new RoomsDataMessage(RoomsDataMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.ROOM_DATAS, roomDatasRequest, roomDatasRequest.GetMessageSize(), RoomsDataMessage.GetHeaderSize());
        }

        protected virtual void ProcessNoticeMessage(IPEndPoint ip, byte[] data)
        {
            NoticeMessage noticeMessage = new NoticeMessage(NoticeMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.NOTICE, noticeMessage, NoticeMessage.GetMessageSize(), NoticeMessage.GetHeaderSize());
        }

        protected virtual void ProcessGameMessage(IPEndPoint ip, byte[] data)
        {
            GameMessage gameMessage = new GameMessage(GameMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.GAME_MESSAGE, gameMessage, GameMessage.GetMessageSize(), GameMessage.GetHeaderSize());
        }

        protected virtual void ProcessChatMessage((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            if (!ipToId.ContainsKey(clientConnectionData))
            {
                return;
            }

            StringMessage stringMessage = new StringMessage(StringMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.STRING, stringMessage, stringMessage.GetMessageSize(), StringMessage.GetHeaderSize());
        }
        #endregion

        #region PRIVATE_METHODS
        private void UpdateSavedMessagesTimes()
        {
            Dictionary<int, float> lastMessageTimes = new Dictionary<int, float>();

            foreach (var messages in lastSemiTcpMessages)
            {
                lastMessageTimes.Add(messages.Key, messages.Value.time - Time.deltaTime);
            }

            foreach (var messageTime in lastMessageTimes)
            {
                if (lastMessageTimes[messageTime.Key] < 0)
                {
                    lastSemiTcpMessages.Remove(messageTime.Key);
                }
                else
                {
                    lastSemiTcpMessages[messageTime.Key] = (lastSemiTcpMessages[messageTime.Key].data, messageTime.Value);
                }
            }
        }

        private void CheckIfMessageIdIsCorrect((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data, MESSAGE_TYPE messageType)
        {
            int clientId = ipToId[clientConnectionData];

            int messageId = MessageFormater.GetMessageId(data);
            Dictionary<MESSAGE_TYPE, int> lastMessagesIds = clients[clientId].lastMessagesIds;

            if (lastMessagesIds.ContainsKey(messageType))
            {
                if (lastMessagesIds[messageType] <= messageId)
                {
                    lastMessagesIds[messageType] = messageId;
                }
                else
                {
                    Debug.Log("Received old message, was erased");
                    wasLastMessageSane = false;
                }
            }
            else
            {
                lastMessagesIds.Add(messageType, messageId);
            }
        }
        #endregion

        #region AUX
        protected virtual void HandleMessageError(byte[] data, int type, SemiTcpMessage semiTcpMessage, int messageSize, int headerSize)
        {
            wasLastMessageSane = CheckMessageSanity(data, headerSize, messageSize, semiTcpMessage, semiTcpMessage.GetMessageTail().messageOperationResult);
            ThrowInsaneMessageLogIfInsane(type);
        }
        #endregion
    }
}
