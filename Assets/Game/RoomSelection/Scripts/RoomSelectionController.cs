using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary;
using System.Net;

namespace Game.RoomSelection
{
    public class RoomSelectionController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private RoomSelectionView roomSelectionView = null;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            roomSelectionView.Init(OnGoBack, OnEnterRoom, OnCreateRoom);

            //get roomDatas from matchMaker
            RoomData roomData = new RoomData(0, 0, 2, 0);

            roomSelectionView.CreateRoomViews(new List<RoomData> { roomData });
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnGoBack()
        {
            ChangeScene(SCENES.LOGIN);
        }

        private void OnEnterRoom()
        {
            RoomData selectedRoomData = roomSelectionView.SelectedRoomData;
            
            if (selectedRoomData != null)
            {
                IPAddress ipAddress = IPAddress.Parse(MatchMaker.ip);
                int port = MatchMaker.matchMakerPort;

                clientHandler.StartClient(ipAddress, port, new RoomData(selectedRoomData.Id, selectedRoomData.PlayersIn, selectedRoomData.PlayersMax, selectedRoomData.MatchTime));

                ChangeScene(SCENES.WAIT_ROOM);
            }
        }

        private void OnCreateRoom()
        {
            ChangeScene(SCENES.MATCH_CONFIGURATION);
        }
        #endregion
    }
}