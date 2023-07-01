using UnityEngine;

namespace Game.Match.Entity.Camera
{
    public class CameraController : MonoBehaviour
    {
        #region PRIVATE_FIELDS
        private Transform player = null;
        #endregion

        #region UNITY_CALLS
        public void Update()
        {
            if (player == null)
            {
                return;
            }

            Vector3 playerPos = player.position;
            playerPos.z = -10;
            transform.position = playerPos;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Transform player)
        {
            this.player = player;
        }
        #endregion
    }
}
