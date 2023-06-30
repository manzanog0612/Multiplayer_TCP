using System.Collections;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class AnimationController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private SpriteRenderer spriteRenderer = null;

        [SerializeField] private Sprite hitSprite = null;
        [SerializeField] private Sprite idleSprite = null;

        [SerializeField] private AudioSource hitSound = null;
        [SerializeField] private AudioSource hurtSound = null;
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

        public void PlayHurtAnimation()
        {
            StartCoroutine(PlayHurtView());
            hurtSound.Play();
        }
        #endregion

        #region PRIVATE_METHODS
        private IEnumerator PlayHurtView()
        {
            float animationDuration = 0.5f;
            Color normalColor = spriteRenderer.color;
            Color hurtColor = spriteRenderer.color;
            hurtColor.a = 0.5f;

            int totalStages = 3;
            int actualStage = 0;

            float[] timeStages = new float[totalStages];

            for (int i = 0; i < totalStages; i++)
            {
                timeStages[i] = (animationDuration / totalStages) * i;
            }

            float time = 0;

            while (time < animationDuration)
            {
                if (time > timeStages[actualStage] && actualStage < totalStages - 1)
                {
                    actualStage++;

                    spriteRenderer.color = spriteRenderer.color == normalColor ? hurtColor : normalColor;
                }

                time += Time.deltaTime;

                yield return null;
            }

            spriteRenderer.color = normalColor;
        }
        #endregion
    }
}
