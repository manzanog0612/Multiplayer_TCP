using Game.Common.Requests;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Formater;
using System.Net;

namespace Game.Common.Networking
{
    public class ClientGameNetwork : ClientNetworkManager
    {
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
        private void ProcessRoomDatasMessage(byte[] data)
        {
            RoomsDataMessage roomDatasMessage = new RoomsDataMessage(RoomsDataMessage.Deserialize(data));
            HandleMessageError(data, (int)MESSAGE_TYPE.ROOM_DATAS, roomDatasMessage, roomDatasMessage.GetMessageSize(), ClientsListMessage.GetHeaderSize());

        }
        #endregion
    }
}
