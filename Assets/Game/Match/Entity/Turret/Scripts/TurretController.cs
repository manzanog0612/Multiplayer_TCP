using Game.Match.Entity.Player;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Match.Entity.Turret
{
    public class TurretController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TurretAnimationController animationController = null;
        [SerializeField] private Transform gun = null;

        [SerializeField] private LayerMask playerLayerMask = 0;
        
        [Header("Hit Configuration")]
        [SerializeField] private GameObject bulletPrefab = null;

        [Header("Animation Configuration")]
        [SerializeField] private float rotationDuration = 0.3f;
        [SerializeField] private float shootAnimDuration = 0.2f;
        #endregion

        #region PRIVATE_FIELDS
        private Transform bulletsHolder = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Transform bulletsHolder)
        {
            this.bulletsHolder = bulletsHolder;
        }

        public void ShootCharacter(Transform character)
        {
            Vector2 direction = (transform.position - character.position).normalized;

            StartCoroutine(Rotate(Quaternion.LookRotation(direction, -Vector3.forward), Shoot));
        }
        #endregion

        #region PRIVATE_METHODS
        private void Shoot()
        {
            animationController.PlayGunShot();
            Debug.Log("SHOOT");
            StartCoroutine(GoBackToIdleInSeconds(shootAnimDuration));
        }

        private IEnumerator Rotate(Quaternion toDir, Action callback)
        {
            float t = 0;

            Quaternion initialRot = transform.transform.rotation;

            while (t < rotationDuration)
            {
                t += Time.deltaTime;

                transform.transform.rotation = Quaternion.Lerp(initialRot, toDir, t);

                yield return null;
            }

            callback.Invoke();
        }

        private IEnumerator GoBackToIdleInSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            animationController.PlayIdle();
        }
        #endregion
    }
}
