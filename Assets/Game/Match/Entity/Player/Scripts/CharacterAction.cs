using System;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterAction
    {
        public bool doAction = false;
        public bool canDoAction = true;
        public float actionCooldownTimer = 0;
        public bool updateCooldown = false;

        public void SetAction(float coolDown)
        {
            doAction = true;
            canDoAction = false;
            actionCooldownTimer = coolDown;
            this.updateCooldown = false;
        }

        public void UpdateCooldown(Action onFinishedCooldown = null)
        {
            if (canDoAction || !updateCooldown)
            {
                return;
            }

            actionCooldownTimer -= Time.fixedDeltaTime;

            if (actionCooldownTimer < 0)
            {
                onFinishedCooldown?.Invoke();
                canDoAction = true;
            }
        }
    }
}
