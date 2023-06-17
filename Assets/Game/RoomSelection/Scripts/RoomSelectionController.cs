using Game.Common;
using Game.RoomSelection.RoomsView;
using UnityEngine;

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

            clientHandler.StartClient();

            clientHandler.SendRoomsDataRequest(
                onReceiveRoomDatas: (roomsDatas) =>
                {
                    roomSelectionView.CreateRoomViews(roomsDatas);
                });
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
                clientHandler.StartMatchMakerConnection(new RoomData(selectedRoomData.Id, selectedRoomData.PlayersIn, selectedRoomData.PlayersMax, selectedRoomData.MatchTime));

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