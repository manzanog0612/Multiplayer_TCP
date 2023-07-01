using Game.Match.Entity.Player;
using System;
using UnityEngine;

namespace Game.Match.Entity.Bullet
{
    public class BulletController : MonoBehaviour
    {
        [SerializeField] ObjectCollider objectCollider = null;
        public int id = 0;

        public void Init(Action<Collider2D, int> onColliderEnter)
        {
            objectCollider.Init((collision) => 
            { 
                onColliderEnter(collision, id);  
                Destroy(gameObject);
            });
        }

        public void SetPosition(Vector2 pos)
        {
            transform.position = pos;
        }
    }
}