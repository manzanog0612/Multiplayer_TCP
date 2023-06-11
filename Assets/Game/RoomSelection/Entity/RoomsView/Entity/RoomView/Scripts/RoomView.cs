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
        #endregion

        #region PUBLIC_FIELDS
        public void Configure(RoomViewModel viewModel, Action onSelect)
        {
            txtMatchName.text = viewModel.RoomName;
            txtPlayers.text = "players " + viewModel.PlayersIn + "/" + viewModel.PlayersMax;
            txtInMatch.SetActive(viewModel.InMatch);
            txtWaiting.SetActive(!viewModel.InMatch);

            button.interactable = !viewModel.InMatch;
            button.onClick.AddListener(() => onSelect.Invoke());
        }
        #endregion
    }
}