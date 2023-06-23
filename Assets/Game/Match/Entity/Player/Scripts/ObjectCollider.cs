using System;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class ObjectCollider : MonoBehaviour
    {
        #region ACTIONS
        private Action<Collider2D> onColliderEnter = null;
        private Action<Collider2D> onColliderStay = null;
        private Action<Collider2D> onColliderExit = null;
        #endregion

        #region UNITY_CALLS
        private void OnTriggerEnter2D(Collider2D collision)
        {
            onColliderEnter.Invoke(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            onColliderStay.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            onColliderExit.Invoke(collision);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action<Collider2D> onColliderEnter = null, Action<Collider2D> onColliderStay = null, Action<Collider2D> onColliderExit = null)
        {
            this.onColliderEnter = onColliderEnter;
            this.onColliderStay = onColliderStay;
            this.onColliderExit = onColliderExit;
        }
        #endregion
    }
}
