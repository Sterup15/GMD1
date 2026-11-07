using GameObjects.Common.Events;
using GameObjects.Common.Pickups.Scripts;
using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable, IHealable
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsFullHealth => CurrentHealth >= MaxHealth;

        // Set by PlayerInvulnerability (after a hit, during a dash)
        private bool _invulnerable;

        private Stats _stats;
        private ActorEvents _events;

        private void Awake()
        {
            _stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            MaxHealth = Mathf.RoundToInt(_stats.MaxHealth.Value);
            CurrentHealth = MaxHealth;
            _events.HealthChanged(CurrentHealth, MaxHealth);
        }

        private void OnEnable()
        {
            _stats.MaxHealth.OnValueChanged += OnMaxHealthUpgraded;
            _events.OnInvulnerableChanged   += SetInvulnerable;
        }

        private void OnDisable()
        {
            _stats.MaxHealth.OnValueChanged -= OnMaxHealthUpgraded;
            _events.OnInvulnerableChanged   -= SetInvulnerable;
            _invulnerable = false;
        }

        private void SetInvulnerable(bool invulnerable) => _invulnerable = invulnerable;

        private void OnMaxHealthUpgraded()
        {
            int newMax = Mathf.RoundToInt(_stats.MaxHealth.Value);
            int diff = newMax - MaxHealth;
            MaxHealth = newMax;
            CurrentHealth = Mathf.Min(CurrentHealth + diff, MaxHealth);
            _events.HealthChanged(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0 || _invulnerable) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            _events.HealthChanged(CurrentHealth, MaxHealth);

            if (CurrentHealth > 0)
            {
                // PlayerInvulnerability starts the safe window on this, before any further hit this frame
                _events.Hit();
                return;
            }

            _events.Death();
            GlobalEvents.PlayerDied();
        }

        public void Heal(int amount)
        {
            if (CurrentHealth <= 0 || IsFullHealth) return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            _events.HealthChanged(CurrentHealth, MaxHealth);
            _events.Healed();
        }
    }
}
