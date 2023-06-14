using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace Game.RoomSelection.RoomsView
{
    public class RoomView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TextMeshProUGUI txtMatchName = null;
        [SerializeField] private TextMeshProUGUI txtPlayers = null;
        [SerializeField] private GameObject txtInMatch = null;
        [SerializeField] private GameObject txtWaiting = null;
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject imgSelected = null;
        #endregion

        #region PUBLIC_FIELDS
        public void Configure(RoomData data, Action<RoomData, RoomView> onSelect)
        {
            txtMatchName.text = "match " + data.Id;
            txtPlayers.text = "players " + data.PlayersIn + "/" + data.PlayersMax;
            txtInMatch.SetActive(data.InMatch);
            txtWaiting.SetActive(!data.InMatch);

            button.interactable = !data.InMatch;
            button.onClick.AddListener(() =>
            {
                onSelect.Invoke(data, this);
                ToggleSelected(true);
            });

            ToggleSelected(false);
        }

        public void ToggleSelected(bool status)
        {
            imgSelected.SetActive(status);
        }
        #endregion
    }
}