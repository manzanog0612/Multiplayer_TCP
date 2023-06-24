using Game.Match.Entity.Turret;
using System.Collections.Generic;
using UnityEngine;

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
        #endregion

        #region PUBLIC_METHODS
        public void Init()
        {
            SpawnTurrets();
        }

        #endregion

        #region PRIVATE_METHODS
        private void SpawnTurrets()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                TurretController turretController = Instantiate(turretPrefab, spawnPoints[i].transform.position, Quaternion.identity, turretsHolder).GetComponent<TurretController>();
                turretController.Init(bulletsHolder);
                turrets.Add(turretController);
            }
        }
        #endregion
    }
}