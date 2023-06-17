using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace Game.Match
{
    public class MatchView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TextMeshProUGUI txtTimer = null;
        #endregion

        #region PRIVATE_FIELDS
        private float time = 0;
        #endregion

        #region UNITY_CALLS
        private void Update()
        {
            time -= Time.deltaTime;
            UpdateTimer();
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(int matchTime)
        {
            time = matchTime;
            UpdateTimer();
        }
        #endregion

        #region PRIVATE_METHODS
        private void UpdateTimer()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            txtTimer.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        #endregion
    }
}