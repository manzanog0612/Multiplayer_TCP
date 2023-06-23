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

        #region PRIVATE_METHODS
        public void UpdateTimer(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            txtTimer.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        #endregion
    }
}