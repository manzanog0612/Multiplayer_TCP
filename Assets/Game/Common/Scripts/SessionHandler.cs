using Game.Common.Networking;
using Game.Common.Requests;
using Game.RoomSelection.RoomsView;
using System;
using UnityEngine;

namespace Game.Common
{
    public class SessionHandler : MonoBehaviourSingleton<SessionHandler>
    {
        #region EXPOSED_FIELDS
        [SerializeField] private ClientHandler clientHandler;
        #endregion

        #region PRIVATE_FIELDS
        private RoomData roomData = null;
        #endregion

        #region ACTIONS
        private Action onFullRoom = null;
        private Action onPlayersAmountChange = null;
        private Action<int, GAME_MESSAGE_TYPE> onReceiveGameMessage = null;
        public Action<float> onTimerUpdate = null;
        public Action onMatchFinished = null;
        public Action<int, object> onReceiveMessage = null;
        #endregion

        #region PROPERTIES
        public RoomData RoomData { get => roomData; }
        public ClientHandler ClientHandler { get => clientHandler; }
        #endregion

        #region PUBLIC_METHODS
        public void SetRoomData(RoomData roomData)
        {
            this.roomData = roomData;
        }

        public void SetOnFullRoom(Action onFullRoom)
        {
            this.onFullRoom = onFullRoom;
        }

        public void SetOnPlayersAmountChange(Action onPlayersAmountChange)
        {
            this.onPlayersAmountChange = onPlayersAmountChange;
        }

        public void SetOnReceiveGameMessage(Action<int, GAME_MESSAGE_TYPE> onReceiveGameMessage)
        {
            this.onReceiveGameMessage = onReceiveGameMessage;
        }

        public void SetOnUpdateTimer(Action<float> onTimerUpdate)
        {
            this.onTimerUpdate = onTimerUpdate;
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnFullRoom()
        {
            onFullRoom?.Invoke();
        }

        private void OnPlayersAmountChange(int playersIn)
        {
            roomData.PlayersIn = playersIn;

            onPlayersAmountChange?.Invoke();
        }

        private void OnReceiveGameMessage(int clientId, GAME_MESSAGE_TYPE messageType)
        {
            onReceiveGameMessage.Invoke(clientId, messageType);
        }

        private void OnUpdateTimer(float time)
        {
            onTimerUpdate?.Invoke(time);
        }

        private void OnMatchFinished()
        {
            onMatchFinished.Invoke();
        }

        private void OnReceiveMessage(int messageType, object data)
        {
            onReceiveMessage?.Invoke(messageType, data);
        }
        #endregion

        #region OVERRIDE_METHODS 
        protected override void Initialize()
        {
            base.Initialize();

            clientHandler.SetAcions(SetRoomData, OnFullRoom, OnPlayersAmountChange, OnReceiveGameMessage, 
                                    OnUpdateTimer, OnMatchFinished, OnReceiveMessage);
        }
        #endregion
    }
}
