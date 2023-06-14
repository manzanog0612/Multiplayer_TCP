using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.RoomSelection.RoomsView;

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
            RoomData roomData = new RoomData(0, 0, 2, 0, false);

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
                //enviar al matchMaker el pedido de entrar al server de id selectedRoomData.Id

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