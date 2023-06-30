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

        [Header("Hit Configuration")]
        [SerializeField] private GameObject bulletPrefab = null;

        [Header("Animation Configuration")]
        [SerializeField] private float shootAnimDuration = 0.2f;
        #endregion

        #region PRIVATE_FIELDS
        private Transform bulletsHolder = null;
        #endregion

        #region ACTIONS
        private Action<int, Vector2, Vector2, GameObject> onSpawnBullet = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Transform bulletsHolder, Action<int, Vector2, Vector2, GameObject> onSpawnBullet)
        {
            this.onSpawnBullet = onSpawnBullet;
            this.bulletsHolder = bulletsHolder;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Shoot(int bulletId)
        {
            animationController.PlayGunShot();
            StartCoroutine(GoBackToIdleInSeconds(shootAnimDuration));

            Vector2 pos = gun.position;
            Vector2 dir = gun.up;

            GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.LookRotation(Vector3.forward, dir), bulletsHolder);

            onSpawnBullet.Invoke(bulletId, pos, dir, bullet);
        }
        #endregion

        #region PRIVATE_METHODS
        private IEnumerator GoBackToIdleInSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            animationController.PlayIdle();
        }
        #endregion
    }
}
