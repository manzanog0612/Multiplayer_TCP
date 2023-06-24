using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Match
{
    public class MatchView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TextMeshProUGUI txtTimer = null;
        [SerializeField] private GameObject winTxt = null;
        [SerializeField] private GameObject loseTxt = null;
        [SerializeField] private Button btnGoBack = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(float time, Action onGoBack)
        {
            UpdateTimer(time);

            btnGoBack.onClick.AddListener(() => onGoBack.Invoke());

            winTxt.SetActive(false);
            loseTxt.SetActive(false);
            btnGoBack.gameObject.SetActive(false);
        }

        public void SetResultView(bool result)
        {
            winTxt.SetActive(result);
            loseTxt.SetActive(!result);
            btnGoBack.gameObject.SetActive(true);
        }

        public void UpdateTimer(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            txtTimer.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        #endregion
    }
}