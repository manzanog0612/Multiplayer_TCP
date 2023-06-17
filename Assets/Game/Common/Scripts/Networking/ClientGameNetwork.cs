using Game.Common.Requests;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Formater;
using System;
using System.Net;

namespace Game.Common.Networking
{
    public class ClientGameNetwork : ClientNetworkManager
    {
        #region ACTIONS
        private Action<RoomData[]> onReceiveRoomDatas = null;
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
        protected override void ProcessRoomDatasMessage(IPEndPoint ip, byte[] data)
        {
            RoomsDataMessage roomDatasRequest = new RoomsDataMessage(RoomsDataMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.ROOM_DATAS, roomDatasRequest, roomDatasRequest.GetMessageSize(), RoomsDataMessage.GetHeaderSize());

            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)MESSAGE_TYPE.ROOM_DATAS, ip);
                return;
            }

            RoomData[] roomDatas = RoomsDataMessage.Deserialize(data);

            onReceiveRoomDatas?.Invoke(roomDatas);
        }
        #endregion

        #region SEND_DATA_METHODS
        public void SendRoomDatasRequest(Action<RoomData[]> onReceiveRoomDatas)
        {
            this.onReceiveRoomDatas = onReceiveRoomDatas;

            RoomDatasRequestMessage roomsDataRequestMessage = new RoomDatasRequestMessage(0);
            byte[] data = roomsDataRequestMessage.Serialize();

            SendData(data);
        }
        #endregion
    }
}
