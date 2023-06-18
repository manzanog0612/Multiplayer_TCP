using System;
using System.Collections;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region PRIVATE_FIELDS
        private CharacterController characterController = null;
        private Vector2 movement = Vector2.zero;
        #endregion

        #region ACTIONS
        private Func<double> onGetLatency = null;
        #endregion

        #region UNITY_CALLS
        private void Update()
        {
            if (!Application.isFocused)
            {
                return;
            }

            DetectInput();
        }

        public void FixedUpdate()
        {
            Processinput();

            ResetData();
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(CharacterController characterController, Func<double> onGetLatency)
        {
            this.characterController = characterController;
            this.onGetLatency = onGetLatency;
        }
        #endregion

        #region PRIVATE_METHODS
        private void DetectInput()
        {
            movement = Vector2.zero;

            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            characterController.DetectHitAction(Input.GetKeyDown(KeyCode.Space));
        }

        private void Processinput()
        {
            if (movement != Vector2.zero)
            {
                characterController.Move(movement);
            }

            StartCoroutine(DoAction(characterController.ProcessHitAction)); //it will only hit if the action was triggered
        }

        private void ResetData()
        {
            movement = Vector3.zero;
        }

        private IEnumerator DoAction(Action action)
        {
            yield return new WaitForSeconds((float)onGetLatency.Invoke());
            action.Invoke();
        }
        #endregion
    }
}
