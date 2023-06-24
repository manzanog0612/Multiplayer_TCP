using Game.Match.Data;
using Game.Match.Entity.Player.LiveMeter;
using MultiplayerLibrary.Reflection.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterController : MonoBehaviour, ISync
    {
        #region EXPOSED_FIELDS
        [SerializeField] private CharacterMovement characterMovement = null;
        [SerializeField] private AnimationController animationController = null;
        [SerializeField] private SpriteRenderer bodyRenderer = null;

        [Header("Hit Configuration")]
        [SerializeField] private ObjectCollider objectCollider = null;
        [SerializeField] private LayerMask playerLayerMask = 0;

        [Header("Life Configurations")]
        [SerializeField] private LiveMeterView liveMeterView = null;
        #endregion

        #region PRIVATE_FIELDS
        [SyncField] private CharacterData characterData = new CharacterData();
        private CharacterAction hitAction = new CharacterAction();
        private bool useDeserializedData = false;

        private bool touchingPlayer = false;
        private Collider2D playerTouched = null;
        #endregion

        #region PROPERTIES
        public CharacterAction HitAction { get => hitAction; }
        public int Life { get => characterData.Life; }
        #endregion

        #region CONSTANTS
        private float actionCooldown = 0.6f;
        #endregion

        #region ACTIONS
        private Action<Collider2D> onHitEffective = null;  
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
        public void Init(Color color, Vector2 position, bool useDeserializedData, Action<Collider2D> onHitEffective)
        {
            bodyRenderer.color = color;
            characterMovement.SetPosition(position);
            this.useDeserializedData = useDeserializedData;
            this.onHitEffective = onHitEffective;

            objectCollider.Init(OnCollision, OnCollision, OnColliderExit);

            characterData.SetLife(MatchConstants.initialLife);
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
                hitAction.UpdateCooldown(OnFinishHit);
            }
        }

        public void Hit(bool doDamage)
        {
            animationController.PlayHitAnim();
            hitAction.updateCooldown = true;

            if (touchingPlayer && doDamage)
            {
                onHitEffective.Invoke(playerTouched);
            }            
        }

        public void Move(Vector2 movement)
        {
            characterMovement.MoveCharacter(movement);
        }

        public void TakeHit()
        {
            characterData.LoseLife(MatchConstants.hitDamage);
            liveMeterView.SetLive((float)characterData.Life / MatchConstants.initialLife);
            Debug.Log("HIT TAKEN, LIFE IS " + characterData.Life + " " + gameObject.name);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnFinishHit()
        {
            animationController.PlayIdle();
        }

        private void OnCollision(Collider2D collision)
        {
            touchingPlayer = ((1 << collision.gameObject.layer) & playerLayerMask) != 0;
            playerTouched = collision;
        }

        private void OnColliderExit(Collider2D collision)
        {
            touchingPlayer = false;
            playerTouched = null;
        }

        private IEnumerator UpdateAutomatic()
        {
            Hit(false);

            while (hitAction.actionCooldownTimer > 0)
            {
                hitAction.UpdateCooldown();
                yield return null;
            }

            OnFinishHit();
            hitAction.canDoAction = true;
        }
        #endregion
    }
}
