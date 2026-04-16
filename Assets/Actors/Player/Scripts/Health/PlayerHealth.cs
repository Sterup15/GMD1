using System;
using Actors.Common;
using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public static Action<int, int> OnHealthChanged; // current, max
        public static Action OnPlayerDied;

        private Stats _stats;

        private void Awake()
        {
            _stats = GetComponent<Stats>();
            MaxHealth = Mathf.RoundToInt(_stats.MaxHealth.Value);
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void OnEnable()  => _stats.MaxHealth.OnValueChanged += OnMaxHealthUpgraded;
        private void OnDisable() => _stats.MaxHealth.OnValueChanged -= OnMaxHealthUpgraded;

        private void OnMaxHealthUpgraded()
        {
            int newMax = Mathf.RoundToInt(_stats.MaxHealth.Value);
            int diff = newMax - MaxHealth;
            MaxHealth = newMax;
            CurrentHealth = Mathf.Min(CurrentHealth + diff, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth == 0)
                OnPlayerDied?.Invoke();
        }
    }
}