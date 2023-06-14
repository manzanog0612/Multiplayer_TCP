using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterMovement : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Rigidbody2D body = null;
        [SerializeField] private float movementSpeed = 5;
        [SerializeField] private float turnSpeed = 5;
        #endregion

        #region PUBLIC_METHOD
        public void MoveCharacter(Vector2 direction)
        {
            body.velocity = direction.normalized * movementSpeed;

            Rotateplayer(direction);
        }

        public void ModifyMovementSpeed(float movementSpeed)
        {
            this.movementSpeed = movementSpeed;
        }

        public void ModifyTurnSpeed(float turnSpeed)
        {
            this.turnSpeed = turnSpeed;
        }
        #endregion

        #region PRIVATE_METHODS
        private void Rotateplayer(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        }
        #endregion
    }
}