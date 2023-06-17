using Game.Common.Networking;
using Game.Common;
using Game.MatchConfiguration;
using Game.RoomSelection.RoomsView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Match
{
    public class MatchController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private MatchView matchView = null;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            matchView.Init(sessionHandler.RoomData.MatchTime);
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