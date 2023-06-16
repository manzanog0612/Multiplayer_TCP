using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Game.RoomSelection.RoomsView;
using Game.MatchConfiguration.Entity.ConfigurableNumber;

namespace Game.MatchConfiguration
{
    public class MatchConfigurationView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Button btnBack = null;
        [SerializeField] private Button btnAccept = null;

        [SerializeField] private List<ConfigurableNumberView> timeNumbers = new List<ConfigurableNumberView>();
        [SerializeField] private ConfigurableNumberView playerNumber = null;
        #endregion

        #region PRIVATE_FIELDS
        private List<int> numbers = new List<int>();
        #endregion

        #region ACTION
        private Action<RoomData> onAccept = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onGoBack, Action<RoomData> onAccept)
        {
            btnBack.onClick.AddListener(() => onGoBack.Invoke());
            btnAccept.onClick.AddListener(() => OnPressAccept());

            this.onAccept = onAccept;

            for (int i = 0; i < timeNumbers.Count; i++)
            {
                numbers.Add(0);

                timeNumbers[i].Init(
                    onPress: (number, id) =>
                    {
                        numbers[id] = number;
                    }, 
                    i);
            }

            playerNumber.Init(null);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPressAccept()
        {
            int matchTime = numbers[0] * 600 + numbers[1] * 60 + numbers[2] * 10 + numbers[3];

            RoomData roomData = new RoomData(-1, 0, playerNumber.ActualNum, matchTime);

            onAccept.Invoke(roomData);
        }
        #endregion
    }
}
