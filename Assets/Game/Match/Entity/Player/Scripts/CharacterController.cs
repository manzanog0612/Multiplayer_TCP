using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private CharacterMovement characterMovement = null;
        [SerializeField] private AnimationController animationController = null;
        [SerializeField] private SpriteRenderer bodyRenderer = null;
        #endregion

        #region PRIVATE_FIELDS
        private CharacterAction hitAction = new CharacterAction();
        #endregion

        #region PROPERTIES
        public CharacterAction HitAction { get => hitAction; }
        #endregion

        #region CONSTANTS
        private float actionCooldown = 0.6f;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Color color, Vector2 position)
        {
            bodyRenderer.color = color;
            characterMovement.SetPosition(position);
        }

        public void DetectHitAction(bool input)
        {
            if (input)
            {
                if (hitAction.canDoAction)
                {
                    hitAction.SetAction(actionCooldown);
                }
            }
            else
            {
                hitAction.UpdateCooldown(animationController.PlayIdle);
            }
        }

        public void ProcessHitAction()
        {
            if (hitAction.doAction)
            {
                Hit();
            }
        }

        public void Hit()
        {
            hitAction.doAction = false;
            animationController.PlayHitAnim();
        }

        public void Move(Vector2 movement)
        {
            characterMovement.MoveCharacter(movement);
        }
        #endregion

        #region PRIVATE_METHODS
        
        #endregion
    }
}
