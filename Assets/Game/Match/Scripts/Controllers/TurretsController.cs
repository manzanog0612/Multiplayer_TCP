using Game.Common.Requests;
using Game.Match.Entity.Bullet;
using Game.Match.Entity.Turret;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Match.Controllers
{
    public class TurretsController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Transform turretsHolder = null;
        [SerializeField] private Transform bulletsHolder = null;
        [SerializeField] private Transform[] spawnPoints = null;
        [SerializeField] private GameObject turretPrefab = null;
        #endregion

        #region PRIVATE_FIELDS
        private List<TurretController> turrets = new List<TurretController>();
        private List<BulletController> bullets = new List<BulletController>();
        #endregion

        #region ACTIONS
        private Action<int, Vector2, Vector2> onSpawnBullet = null;
        private Action<int, Collider2D> onSetBulletDeath = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action <int, Vector2, Vector2> onSpawnBullet, Action<int, Collider2D> onSetBulletDeath)
        {
            this.onSpawnBullet = onSpawnBullet;
            this.onSetBulletDeath = onSetBulletDeath;
            SpawnTurrets();
        }

        public void OnReceiveTurretData(GAME_MESSAGE_TYPE messageType, object data)
        {
            switch (messageType)
            {
                case GAME_MESSAGE_TYPE.TURRET_ROTATION:
                    SetTurretRotation(data);
                    break;
                case GAME_MESSAGE_TYPE.TURRET_SHOOT:
                    SetTurretShoot(data);
                    break;
                case GAME_MESSAGE_TYPE.BULLET_POSITION:
                    SetBulletPosition(data);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetBulletPosition(object data)
        {
            (int bulletId, Vector2 position)? result = data as (int, Vector2)?;

            BulletController bullet = bullets.Find(b => b.id == result.Value.bulletId);

            if (bullet != null)
            {
                bullet.SetPosition(result.Value.position);
            }
        }

        private void SetTurretRotation(object data)
        {
            (int turretId, Quaternion rotation)? result = data as (int, Quaternion)?;

            turrets[result.Value.turretId].SetRotation(result.Value.rotation);
        }

        private void SetTurretShoot(object data)
        {
            (int turretId, int bulletId)? result = data as (int, int)?;

            turrets[result.Value.turretId].Shoot(result.Value.bulletId);
        }

        private void SpawnTurrets()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                TurretController turretController = Instantiate(turretPrefab, spawnPoints[i].transform.position, Quaternion.identity, turretsHolder).GetComponent<TurretController>();
                turretController.Init(bulletsHolder, OnShotBullet);
                turrets.Add(turretController);
            }
        }

        private void OnShotBullet(int id, Vector2 pos, Vector2 dir, GameObject bullet)
        {
            if (bullets.Find(b => b.id == id))
            {
                Debug.Log("Bullet already existed, destroyed duplicated one");
                Destroy(bullet);
                return;
            }

            BulletController bulletController =  bullet.GetComponent<BulletController>();
            bulletController.id = id;
            bulletController.Init(OnHit);
            bullets.Add(bulletController);

            onSpawnBullet.Invoke(id, pos, dir);
        }

        private void OnHit(Collider2D collision, int id)
        {
            onSetBulletDeath.Invoke(id, collision);

            BulletController bullet = bullets.Find(b => b.id == id);

            if (bullet != null)
            {
                bullets.Remove(bullet);
                Destroy(bullet.gameObject);
            }
        }
        #endregion
    }
}