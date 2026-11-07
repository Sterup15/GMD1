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
        public event Action<int> OnDamaged;
        public event Action OnHealed;
        // The window after a hit (the player blinks); OnInvulnerableChanged also covers the dash
        public event Action OnHitInvulnerabilityStarted;
        public event Action OnHitInvulnerabilityEnded;
        public event Action<bool> OnInvulnerableChanged;
        public event Action<Vector2> OnRetreat;
        public event Action<Vector2> OnDashStarted;
        public event Action OnDashEnded;
        // Raised by the player's movement state when an attack starts or stops, for talents
        public event Action<bool> OnAttackingChanged;
        // Raised by attacks, so talents can add to them: (origin, direction) and (position, damage, radius)
        public event Action<Vector2, Vector2> OnProjectileFired;
        public event Action<Vector2, int, float> OnBombExploded;

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
        public void Damaged(int amount) => OnDamaged?.Invoke(amount);
        public void Healed()          => OnHealed?.Invoke();
        public void HitInvulnerabilityStarted() => OnHitInvulnerabilityStarted?.Invoke();
        public void HitInvulnerabilityEnded()   => OnHitInvulnerabilityEnded?.Invoke();
        public void InvulnerableChanged(bool invulnerable) => OnInvulnerableChanged?.Invoke(invulnerable);
        public void Retreat(Vector2 direction) => OnRetreat?.Invoke(direction);
        public void DashStarted(Vector2 velocity)  => OnDashStarted?.Invoke(velocity);
        public void DashEnded()                    => OnDashEnded?.Invoke();
        public void AttackingChanged(bool attacking) => OnAttackingChanged?.Invoke(attacking);
        public void ProjectileFired(Vector2 origin, Vector2 direction)    => OnProjectileFired?.Invoke(origin, direction);
        public void BombExploded(Vector2 position, int damage, float radius) => OnBombExploded?.Invoke(position, damage, radius);
    }
}
