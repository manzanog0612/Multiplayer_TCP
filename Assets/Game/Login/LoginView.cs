using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Login
{
    public class LoginView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TMP_InputField inputFieldPlayerName = null;
        [SerializeField] private Button btnLogin = null;
        #endregion

        #region PROPERTIES
        public bool LoginButtonsIsOn => btnLogin.gameObject.activeSelf;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action<string> onPlayerNameChanged, Action<string> onPlayerLogin)
        {
            inputFieldPlayerName.onValueChanged.AddListener((s) => onPlayerNameChanged.Invoke(s));

            btnLogin.onClick.AddListener(() => onPlayerLogin.Invoke(inputFieldPlayerName.text));

            onPlayerNameChanged.Invoke("ddddd");//debug
        }

        public void ToggleLoginButton(bool status)
        {
            btnLogin.gameObject.SetActive(status);
        }
        #endregion
    }
}
