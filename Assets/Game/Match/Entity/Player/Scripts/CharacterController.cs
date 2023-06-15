using System;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterAction
    {
        public bool doAction = false;
        public bool canDoAction = true;
        public float actionCooldownTimer = 0;

        public void SetAction(float coolDown)
        {
            doAction = true;
            canDoAction = false;
            actionCooldownTimer = coolDown;
        }

        public void UpdateCooldown(Action onFinishedCooldown)
        {
            if (canDoAction)
            {
                return;
            }

            if (actionCooldownTimer > 0)
            {
                actionCooldownTimer -= Time.fixedDeltaTime;
            }
            else
            {
                onFinishedCooldown?.Invoke();
                canDoAction = true;
            }
        }
    }

    public class CharacterController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private CharacterMovement characterMovement = null;
        [SerializeField] private AnimationController animationController = null;
        #endregion

        #region PRIVATE_FIELDS
        private Vector2 movement = Vector2.zero;

        private CharacterAction hit = new CharacterAction();
        #endregion

        #region CONSTANTS
        private float actionCooldown = 0.6f;
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
        public void Hit()
        {
            hit.doAction = false;
            animationController.PlayHitAnim();
        }
        #endregion

        #region PRIVATE_METHODS
        private void DetectInput()
        {
            movement = Vector2.zero;

            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (hit.canDoAction)
                {
                    hit.SetAction(actionCooldown);
                    Debug.Log("HitStart");
                }
            }
            else
            {
                hit.UpdateCooldown(animationController.PlayIdle);
            }
        }

        private void Processinput()
        {
            if (movement != Vector2.zero)
            {
                characterMovement.MoveCharacter(movement);
            }

            if (hit.doAction)
            {
                Hit();
            }
        }

        private void ResetData()
        {
            movement = Vector3.zero;
        }
        #endregion
    }
}
