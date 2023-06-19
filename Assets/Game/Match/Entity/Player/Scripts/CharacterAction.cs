using System;
using UnityEngine;

namespace Game.Match.Entity.Player
{
    public class CharacterAction
    {
        public bool doAction = false;
        public bool canDoAction = true;
        public float actionCooldownTimer = 0;

        public void SetAction(float coolDown)
        {
            doAction = true;
            canDoAction = false;
            actionCooldownTimer = coolDown;
        }

        public void UpdateCooldown(Action onFinishedCooldown = null)
        {
            if (canDoAction)
            {
                return;
            }

            if (actionCooldownTimer > 0)
            {
                actionCooldownTimer -= Time.fixedDeltaTime;
            }
            else
            {
                onFinishedCooldown?.Invoke();
                canDoAction = true;
            }
        }
    }
}
