using Game.Common;
using UnityEngine;

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

            sessionHandler.SetOnFullRoom(OnGoToMatch);
            sessionHandler.SetOnPlayersAmountChange(
                onPlayersAmountChange: () =>
                {
                    waitRoomView.SetPlayersText(sessionHandler.RoomData.PlayersIn, sessionHandler.RoomData.PlayersMax);
                });

                
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