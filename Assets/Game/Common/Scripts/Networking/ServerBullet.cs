using System;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ServerBullet
    {
        #region PUBLIC_FIELDS
        public Vector2 pos;
        public Vector2 dir;
        public float speed;
        public int id = -1;
        #endregion

        #region CONSTRUCTOR
        public ServerBullet(Vector2 pos, Vector2 dir, float speed, int id)
        {
            this.pos = pos;
            this.dir = dir;
            this.speed = speed;
            this.id = id;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Update()
        {
            pos += dir.normalized * speed * Time.deltaTime;
        }
        #endregion
    }
}
