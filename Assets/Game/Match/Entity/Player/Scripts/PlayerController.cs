using Game.Common.Requests;
using System;
using System.Collections;
using UnityEditor;
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
        private Action<GAME_MESSAGE_TYPE> onSendMessage = null;
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
        public void Init(CharacterController characterController, Func<double> onGetLatency, Action<GAME_MESSAGE_TYPE> onSendMessage)
        {
            this.characterController = characterController;
            this.onGetLatency = onGetLatency;
            this.onSendMessage = onSendMessage;
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

            if (characterController.HitAction.doAction)
            {
                onSendMessage.Invoke(GAME_MESSAGE_TYPE.PLAYER_HIT);
                characterController.HitAction.doAction = false;
                StartCoroutine(DoAction(characterController.Hit));
            }
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
