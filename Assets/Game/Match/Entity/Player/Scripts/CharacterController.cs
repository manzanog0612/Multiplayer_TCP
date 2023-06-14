using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private CharacterMovement characterMovement = null;
        #endregion

        #region PRIVATE_FIELDS
        private Vector2 movement = Vector2.zero;
        #endregion

        #region UNITY_CALLS
        public void FixedUpdate()
        {
            if (!Application.isFocused)
            {
                return;
            }

            DetectInput();

            Processinput();

            ResetData();
        }
        #endregion

        #region PRIVATE_METHODS
        private void DetectInput()
        {
            movement = Vector2.zero;

            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        private void Processinput()
        {
            if (movement != Vector2.zero)
            {
                characterMovement.MoveCharacter(movement);
            }
        }

        private void ResetData()
        {
            movement = Vector3.zero;
        }
        #endregion
    }
}
