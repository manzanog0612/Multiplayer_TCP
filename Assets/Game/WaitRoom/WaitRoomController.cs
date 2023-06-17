using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.RoomSelection.RoomsView;
using Game.RoomSelection;

namespace Game.WaitRoom
{
    public class WaitRoomController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private WaitRoomView waitRoomView = null;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            clientHandler.SetOnEnterRoom(OnGoToMatch);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnGoToMatch()
        {
            ChangeScene(SCENES.MATCH);
        }
        #endregion
    }
}