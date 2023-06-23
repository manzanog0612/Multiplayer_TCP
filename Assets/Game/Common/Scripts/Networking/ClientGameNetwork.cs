using Game.Common.Requests;
using Game.RoomSelection.RoomsView;
using MultiplayerLib2.Network.Message;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Formater;
using MultiplayerLibrary.Network.Message.Constants;
using System;
using System.Collections.Generic;
using System.Net;

namespace Game.Common.Networking
{
    public class ClientGameNetwork : ClientNetworkManager
    {
        #region ACTIONS
        private Action<RoomData> onGetRoomData = null;
        private Action<RoomData[]> onReceiveRoomDatas = null;
        private Action onFullRoom = null;
        private Action<int> onPlayersAmountChange = null;
        private Action<int, GAME_MESSAGE_TYPE> onReceiveGameMessage = null;
        #endregion

        #region ACTIONS
        public Dictionary<int, Client> Clients { get => clients; }
        #endregion

        #region OVERRIDE_METHODS
        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {            
            base.OnReceiveData(data, ip);
            
            bool isReflectionMessage = MessageFormater.IsReflectionMessage(data);

            if (isReflectionMessage)
            {
                return;
            }
            else
            {
                GAME_REQUEST_TYPE messageType = GameMessageFormater.GetMessageType(data);

                switch (messageType)
                {
                    default:
                        break;
                }
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetAcions(Action<RoomData> onGetRoomData, Action onFullRoom, Action<int> onPlayersAmountChange, Action<int, GAME_MESSAGE_TYPE> onReceiveGameMessage)
        {
            this.onGetRoomData = onGetRoomData;
            this.onFullRoom = onFullRoom;
            this.onPlayersAmountChange = onPlayersAmountChange;
            this.onReceiveGameMessage = onReceiveGameMessage;
        }
        #endregion

        #region DATA_RECEIVE_PROCESS
        protected override void ProcessConnectRequest(IPEndPoint ip, byte[] data)
        {
            base.ProcessConnectRequest(ip, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            (long server, int port, RoomData roomData) room = ConnectRequestMessage.Deserialize(data);

            onGetRoomData?.Invoke(room.roomData);
        }

        protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
        {
            base.ProcessEntityDisconnect(ip, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            onPlayersAmountChange?.Invoke(clients.Count);
        }

        protected override void ProcessClientList(byte[] data)
        {
            base.ProcessClientList(data);
        
            if (!wasLastMessageSane)
            {
                return;
            }

            onPlayersAmountChange?.Invoke(clients.Count);
        }

        protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            base.ProcessHandShake(clientConnectionData, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            onPlayersAmountChange?.Invoke(clients.Count);
        }

        protected override void ProcessRoomDatasMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessRoomDatasMessage(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.ROOM_DATAS, ip);
                return;
            }

            RoomData[] roomDatas = RoomsDataMessage.Deserialize(data);

            onReceiveRoomDatas?.Invoke(roomDatas);
        }

        protected override void ProcessNoticeMessage(IPEndPoint ip, byte[] data)
        {
            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.NOTICE, ip);
                return;
            }

            NOTICE notice = (NOTICE)NoticeMessage.Deserialize(data);

            switch (notice)
            {
                case NOTICE.ROOM_REQUEST:
                    break;
                case NOTICE.FULL_ROOM:
                    onFullRoom.Invoke();
                    break;
                default:
                    break;
            }
        }

        protected override void ProcessGameMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessGameMessage(ip, data);

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.GAME_MESSAGE, ip);
                return;
            }

            (int clientId, int message) = GameMessage.Deserialize(data);

            if (!clients.ContainsKey(clientId))
            {
                return;
            }

            onReceiveGameMessage?.Invoke(clientId, (GAME_MESSAGE_TYPE)message);
        }
        #endregion

        #region SEND_DATA_METHODS
        public void SendGameMessage(int clientId, GAME_MESSAGE_TYPE messageType)
        {
            GameMessage gameMessage = new GameMessage((clientId, (int)messageType));
            byte[] data = gameMessage.Serialize();

            OnSendData(MESSAGE_TYPE.GAME_MESSAGE, data);
        }

        public void SendRoomDatasRequest(Action<RoomData[]> onReceiveRoomDatas)
        {
            this.onReceiveRoomDatas = onReceiveRoomDatas;

            NoticeMessage noticeMessage = new NoticeMessage((int)NOTICE.ROOM_REQUEST);
            byte[] data = noticeMessage.Serialize();

            OnSendData(MESSAGE_TYPE.NOTICE, data);
        }
        #endregion
    }
}
