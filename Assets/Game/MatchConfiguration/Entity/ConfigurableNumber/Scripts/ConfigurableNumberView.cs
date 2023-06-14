using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace Game.MatchConfiguration.Entity.ConfigurableNumber
{
    public class ConfigurableNumberView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [Header("Limits")]
        [SerializeField] private int minNum = 0;
        [SerializeField] private int maxNum = 9;

        [Header("Configurations")]
        [SerializeField] private TextMeshProUGUI txtNum = null;
        [SerializeField] private Button btnUp = null;
        [SerializeField] private Button btnDown = null;
        #endregion

        #region ACTIONS
        private Action<int, int> onPress = null;
        #endregion

        #region PRPERTIES
        public int ActualNum { private set; get; }
        public int Id { private set; get; }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action<int, int> onPress, int id = 0)
        {
            btnUp.onClick.AddListener(() => OnButtonUp());
            btnDown.onClick.AddListener(() => OnButtonDown());

            ActualNum = minNum;
            Id = id;

            this.onPress = onPress;

            OnPress();
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnButtonUp()
        {
            ActualNum++;

            if (ActualNum > maxNum)
            {
                ActualNum = minNum;
            }

            OnPress();
        }

        private void OnButtonDown()
        {
            ActualNum--;

            if (ActualNum < minNum)
            {
                ActualNum = maxNum;
            }

            OnPress();
        }

        private void OnPress()
        {
            txtNum.text = ActualNum.ToString();
            onPress?.Invoke(ActualNum, Id);
        }
        #endregion
    }
}