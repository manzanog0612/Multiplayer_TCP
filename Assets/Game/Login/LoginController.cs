using UnityEngine;

using Game.Common;

namespace Game.Login
{
    public class LoginController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private LoginView loginView = null;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            loginView.Init(OnPressLogin);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPressLogin()
        {
            ChangeScene(SCENES.ROOM_SELECTION);
        }
        #endregion
    }
}

