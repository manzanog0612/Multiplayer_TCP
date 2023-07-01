using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Login
{
    public class LoginView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Button btnLogin = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onPlayerLogin)
        {
            btnLogin.onClick.AddListener(() => onPlayerLogin.Invoke());
        }
        #endregion
    }
}
