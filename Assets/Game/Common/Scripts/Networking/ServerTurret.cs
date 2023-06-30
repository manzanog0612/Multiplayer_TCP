using System;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ServerTurret
    {
        #region PUBLIC_FIELDS
        public Vector2 pos;
        public Quaternion rot;
        public bool rotating = false;
        #endregion

        #region PRIVATE_FIELDS
        private int id = -1;

        //rotation anim stuff
        private float rotTimer = 0;
        private float duration = 0;
        private Quaternion fromRot;
        private Quaternion toRot;

        //shoot stuff
        private bool canShoot = false;
        private float shootTimer = 0;
        private float cooldown = 0;
        private static int bulletId = 0;
        #endregion

        #region ACTIONS
        private Action<int, int> onShoot = null;
        private Action<int, Quaternion> onRotate = null;
        #endregion

        #region CONSTRUCTOR
        public ServerTurret(int id, Vector2 pos, Quaternion rot, float cooldown, Action<int, int> onShoot, Action<int, Quaternion> onRotate)
        {
            this.id = id;
            this.pos = pos;
            this.rot = rot;
            this.cooldown = cooldown;
            this.onShoot = onShoot;
            this.onRotate = onRotate;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Update()
        {
            if (rotating && rotTimer < duration)
            {
                rotTimer += Time.deltaTime;

                rot = Quaternion.Lerp(fromRot, toRot, rotTimer);

                onRotate.Invoke(id, rot);

                if (rotTimer > duration)
                {
                    rotating = false;
                    canShoot = true;
                }
            }

            if (canShoot && shootTimer < cooldown)
            {
                shootTimer += Time.deltaTime;
            
                if (shootTimer > cooldown)
                {
                    onShoot.Invoke(id, bulletId);
                    bulletId++;
                    shootTimer = 0;
                }
            }
        }

        public Quaternion GetLookRot(Vector2 toPos)
        {
            Vector2 direction = (toPos - pos).normalized;

            return Quaternion.LookRotation(Vector3.forward, direction);
        }

        public void StartRotation(Quaternion toDir, float rotDuration)
        {
            rotTimer = 0;
            duration = rotDuration;

            fromRot = rot;
            toRot = toDir;

            rotating = true;
        }
        #endregion
    }
}
