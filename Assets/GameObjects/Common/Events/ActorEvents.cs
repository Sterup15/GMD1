using System;
using UnityEngine;

namespace GameObjects.Common.Events
{
    public class ActorEvents : MonoBehaviour
    {
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeathStarted;
        public event Action OnDeath;
        public event Action OnShotFired;
        public event Action OnAttackComplete;
        public event Action OnWindupComplete;
        public event Action OnRecoverComplete;
        public event Action OnHitboxOpen;
        public event Action OnHitboxClose;
        public event Action OnDeathComplete;
        public event Action OnStep;
        public event Action OnSwing;
        public event Action OnHit;

        public bool HasDeathStartedListeners => OnDeathStarted != null;

        public void HealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public void DeathStarted() => OnDeathStarted?.Invoke();
        public void Death()        => OnDeath?.Invoke();
        public void ShotFired()    => OnShotFired?.Invoke();
        public void AttackComplete()  => OnAttackComplete?.Invoke();
        public void WindupComplete()  => OnWindupComplete?.Invoke();
        public void RecoverComplete() => OnRecoverComplete?.Invoke();
        public void HitboxOpen()      => OnHitboxOpen?.Invoke();
        public void HitboxClose()     => OnHitboxClose?.Invoke();
        public void DeathComplete()   => OnDeathComplete?.Invoke();
        public void Step()            => OnStep?.Invoke();
        public void Swing()           => OnSwing?.Invoke();
        public void Hit()             => OnHit?.Invoke();
    }
}
