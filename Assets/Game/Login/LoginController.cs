using UnityEngine;

using Game.Common;

namespace Game.Login
{
    public class LoginController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private LoginView loginView = null;
        [SerializeField] private int minCharactersForName = 3;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            loginView.Init(OnPlayerNameChanged, OnPressLogin);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPlayerNameChanged(string playerName)
        {
            if (playerName.Length < minCharactersForName && loginView.LoginButtonsIsOn)
            {
                loginView.ToggleLoginButton(false);
            }
            else if (playerName.Length >= minCharactersForName && !loginView.LoginButtonsIsOn)
            {
                loginView.ToggleLoginButton(true);
            }
        }

        private void OnPressLogin(string playerName)
        {
            sessionHandler.SetPlayerName(playerName);

            ChangeScene(SCENES.ROOM_SELECTION);
        }
        #endregion
    }
}

