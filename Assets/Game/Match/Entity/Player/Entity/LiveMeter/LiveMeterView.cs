using UnityEngine;

namespace Game.Match.Entity.Player.LiveMeter
{
    public class LiveMeterView : MonoBehaviour
    {
        [SerializeField] private GameObject live;

        private float initialScale = 0;

        private void Start()
        {
            initialScale = live.transform.localScale.x;
        }

        public void SetLive(float live)
        {
            this.live.transform.localScale = new Vector2(initialScale * live, this.live.transform.localScale.y); ;
        }
    }
}