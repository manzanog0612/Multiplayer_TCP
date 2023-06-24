using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Match.Entity.Turret
{
    public class TurretAnimationController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private SpriteRenderer spriteRenderer = null;

        [SerializeField] private Sprite idleSprite = null;
        [SerializeField] private Sprite shootSprite = null;

        [SerializeField] private AudioSource shotSound = null;
        #endregion

        #region PUBLIC_ANIMATION
        public void PlayIdle()
        {
            spriteRenderer.sprite = idleSprite;
        }

        public void PlayGunShot()
        {
            spriteRenderer.sprite = shootSprite;
            shotSound.Play();
        }
        #endregion
    }
}
