using System;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class ObjectCollider : MonoBehaviour
    {
        #region ACTIONS
        private Action<Collider2D> onColliderEnter = null;
        #endregion

        #region UNITY_CALLS
        private void OnTriggerEnter2D(Collider2D collision)
        {
            onColliderEnter.Invoke(collision);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action<Collider2D> onColliderEnter)
        {
            this.onColliderEnter = onColliderEnter;
        }

        public void ToggleView(bool status)
        {
            gameObject.SetActive(status);
        }
        #endregion
    }
}
