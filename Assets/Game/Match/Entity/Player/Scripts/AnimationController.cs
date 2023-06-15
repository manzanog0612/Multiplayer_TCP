using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class AnimationController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private SpriteRenderer spriteRenderer = null;

        [SerializeField] private Sprite hitSprite = null;
        [SerializeField] private Sprite idleSprite = null;
        [SerializeField] private Sprite idleGunSprite = null;
        [SerializeField] private Sprite shootSprite = null;

        [SerializeField] private AudioSource hitSound = null;
        #endregion

        #region PUBLIC_ANIMATION
        public void PlayHitAnim()
        {
            spriteRenderer.sprite = hitSprite;
            hitSound.Play();
        }

        public void PlayIdle()
        {
            spriteRenderer.sprite = idleSprite;
        }

        public void PlayIdleGun()
        {
            spriteRenderer.sprite = idleGunSprite;
        }

        public void PlayGunShot()
        {
            spriteRenderer.sprite = shootSprite;
        }
        #endregion
    }
}
