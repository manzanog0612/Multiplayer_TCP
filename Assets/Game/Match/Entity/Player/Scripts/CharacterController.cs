using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Match.Entity.Player
{
    public class CharacterController : MonoBehaviour, ISync
    {
        #region EXPOSED_FIELDS
        [SerializeField] private CharacterMovement characterMovement = null;
        [SerializeField] private AnimationController animationController = null;
        [SerializeField] private SpriteRenderer bodyRenderer = null;
        #endregion

        #region PRIVATE_FIELDS
        private CharacterAction hitAction = new CharacterAction();
        private bool useDeserializedData = false;
        #endregion

        #region PROPERTIES
        public CharacterAction HitAction { get => hitAction; }
        #endregion

        #region CONSTANTS
        private float actionCooldown = 0.6f;
        #endregion

        #region OVERRIDE_METHODS
        public byte[] Serialize()
        {        
            List<byte> bytes = new List<byte>();
        
            bytes.AddRange(BitConverter.GetBytes(transform.position.x));
            bytes.AddRange(BitConverter.GetBytes(transform.position.y));
            bytes.AddRange(BitConverter.GetBytes(transform.rotation.x));
            bytes.AddRange(BitConverter.GetBytes(transform.rotation.y));
            bytes.AddRange(BitConverter.GetBytes(transform.rotation.z));
            bytes.AddRange(BitConverter.GetBytes(transform.rotation.w));

            return bytes.ToArray();
        }
        
        public void Deserialize(byte[] msg)
        {
            if (!useDeserializedData)
            {
                return;
            }
        
            Vector2 position = new Vector2(BitConverter.ToSingle(msg), BitConverter.ToSingle(msg, sizeof(float)));
            Quaternion rotation = new Quaternion(BitConverter.ToSingle(msg, sizeof(float) * 2),
                                                 BitConverter.ToSingle(msg, sizeof(float) * 3),
                                                 BitConverter.ToSingle(msg, sizeof(float) * 4),
                                                 BitConverter.ToSingle(msg, sizeof(float) * 5));

            transform.position = position;
            transform.rotation = rotation;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Color color, Vector2 position, bool useDeserializedData)
        {
            bodyRenderer.color = color;
            characterMovement.SetPosition(position);
            this.useDeserializedData = useDeserializedData;
        }

        public void DetectHitAction(bool input, bool goBackToIdle = false)
        {
            if (input)
            {
                if (hitAction.canDoAction)
                {
                    hitAction.SetAction(actionCooldown);

                    if (goBackToIdle)
                    {
                        StartCoroutine(UpdateAutomatic());
                    }
                }
            }
            else
            {
                hitAction.UpdateCooldown(animationController.PlayIdle);
            }
        }

        public void Hit()
        {
            animationController.PlayHitAnim();
        }

        public void Move(Vector2 movement)
        {
            characterMovement.MoveCharacter(movement);
        }
        #endregion

        #region PRIVATE_METHODS
        public IEnumerator UpdateAutomatic()
        {
            Hit();

            while (hitAction.actionCooldownTimer > 0)
            {
                hitAction.UpdateCooldown();
                yield return null;
            }

            animationController.PlayIdle();
            hitAction.canDoAction = true;
        }
        #endregion
    }
}
