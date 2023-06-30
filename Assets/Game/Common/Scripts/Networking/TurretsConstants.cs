using System.Collections.Generic;
using UnityEngine;

namespace Game.Common.Networking
{
    public class TurretsConstants
    {
        public const float cooldown = 0.5f;
        public const float rotDuration = 0.1f;
        public const float minDistanceToShoot = 10;
        public const float bulletSpeed = 10f;

        public const float xPosLeft = -20.4f;
        public const float xPosMiddle = 0f;
        public const float xPosRight = 20.4f;
        public const float yPosUp = 14.28f;
        public const float yPosDown = -12f;
        public const float yPosMiddle = 0f;

        public static Vector2[] GetTurretsPos()
        {
            List<Vector2> res = new List<Vector2>();

            res.Add(new Vector2(xPosLeft, yPosUp));
            res.Add(new Vector2(xPosMiddle, yPosUp));
            res.Add(new Vector2(xPosRight, yPosUp));
            res.Add(new Vector2(xPosLeft, yPosDown));
            res.Add(new Vector2(xPosMiddle, yPosDown));
            res.Add(new Vector2(xPosRight, yPosDown));
            res.Add(new Vector2(xPosLeft, yPosMiddle));
            res.Add(new Vector2(xPosMiddle, yPosMiddle));
            res.Add(new Vector2(xPosRight, yPosMiddle));

            return res.ToArray();
        }
    }
}
