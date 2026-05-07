using System;
using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class BossMeleeAnimationEvents : MonoBehaviour
    {
        public event Action OnDeathAnimationComplete;

        private BossMeleeMovementState _movementState;
        private BossMelee _boss;

        private void Awake()
        {
            _movementState = GetComponentInParent<BossMeleeMovementState>();
            _boss = GetComponentInParent<BossMelee>();
        }

        public void OnHitboxOpen() => _boss.OpenHitbox();
        public void OnHitboxClose() => _boss.CloseHitbox();

        // Fired at end of Windup clip — snapshots lunge target and transitions to Attack
        public void OnWindupComplete()
        {
            _boss.OnWindupComplete();
            _movementState.OnWindupComplete();
        }

        // Fired at end of Attack clip — transitions to Recover
        public void OnAttackComplete() => _movementState.OnAttackComplete();

        // Fired at end of Recover clip — returns to normal movement
        public void OnRecoverComplete() => _movementState.OnRecoverComplete();

        // Fired at end of Die clip
        public void OnDeathComplete() => OnDeathAnimationComplete?.Invoke();
    }
}
