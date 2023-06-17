using TMPro;
using UnityEngine;

namespace Game.WaitRoom
{
    public class WaitRoomView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TextMeshProUGUI txtPlayers = null;
        #endregion

        #region 
        public void SetPlayersText(int playersIn, int maxPlayers)
        {
            txtPlayers.text = "players " + playersIn + "/" + maxPlayers;
        }
        #endregion
    }
}