using Game.Common.Networking;
using Game.Common.Player;
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
        private PlayerModel playerModel = null;
        private RoomData roomData = null;
        #endregion

        #region ACTIONS
        private Action onFullRoom = null;
        private Action onPlayersAmountChange = null;
        #endregion

        #region PROPERTIES
        public PlayerModel PlayerModel { get => playerModel; }
        public RoomData RoomData { get => roomData; }
        public ClientHandler ClientHandler { get => clientHandler; }
        #endregion

        #region PUBLIC_METHODS
        public void SetPlayerName(string name)
        {
            playerModel.SetName(name);
        }

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
        #endregion

        #region OVERRIDE_METHODS 
        protected override void Initialize()
        {
            base.Initialize();

            playerModel = new PlayerModel();

            clientHandler.SetAcions(SetRoomData, OnFullRoom, OnPlayersAmountChange);
        }
        #endregion
    }
}
