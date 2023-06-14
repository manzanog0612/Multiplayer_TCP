using UnityEngine;

namespace Game.Match.Entity.Camera
{
    public class CameraController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Transform player = null;
        #endregion

        #region UNITY_CALLS
        public void Update()
        {
            Vector3 playerPos = player.position;
            playerPos.z = -10;
            transform.position = playerPos;
        }
        #endregion
    }
}
