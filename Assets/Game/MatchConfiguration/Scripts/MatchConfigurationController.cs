using UnityEngine;

using Game.Common;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary;
using System.Net;

namespace Game.MatchConfiguration
{
    public class MatchConfigurationController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private MatchConfigurationView matchConfigurationView = null;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            matchConfigurationView.Init(OnGoBack, OnAccept);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnGoBack()
        {
            ChangeScene(SCENES.ROOM_SELECTION);
        }

        private void OnAccept(RoomData roomData)
        {
            clientHandler.StartMatchMakerConnection(new RoomData(-1, 0, roomData.PlayersMax, roomData.MatchTime));

            Debug.Log("room data created with " + roomData.MatchTime + " matchTime & " + roomData.PlayersMax + " maxPlayers");

            ChangeScene(SCENES.WAIT_ROOM);
        }
        #endregion
    }
}