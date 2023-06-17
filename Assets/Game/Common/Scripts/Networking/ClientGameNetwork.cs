using Game.Common.Requests;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Formater;
using MultiplayerLibrary.Network.Message.Constants;
using System;
using System.Net;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ClientGameNetwork : ClientNetworkManager
    {
        #region ACTIONS
        private Action<RoomData> onGetRoomData = null;
        private Action<RoomData[]> onReceiveRoomDatas = null;
        private Action onFullRoom = null;
        private Action<int> onPlayersAmountChange = null;
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
                GAME_MESSAGE_TYPE messageType = GameMessageFormater.GetMessageType(data);

                switch (messageType)
                {
                    default:
                        break;
                }
            }
        }
        #endregion

        #region DATA_RECEIVE_PROCESS

        #region DEBUG
        #endregion
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
        #endregion

        #region SEND_DATA_METHODS
        public void SendRoomDatasRequest(Action<RoomData[]> onReceiveRoomDatas)
        {
            this.onReceiveRoomDatas = onReceiveRoomDatas;

            NoticeMessage noticeMessage = new NoticeMessage((int)NOTICE.ROOM_REQUEST);
            byte[] data = noticeMessage.Serialize();

            SendData(data);
        }

        public void SetAcions(Action<RoomData> onGetRoomData, Action onFullRoom, Action<int> onPlayersAmountChange)
        {
            this.onGetRoomData = onGetRoomData;
            this.onFullRoom = onFullRoom;
            this.onPlayersAmountChange = onPlayersAmountChange;
        }
        #endregion
    }
}
