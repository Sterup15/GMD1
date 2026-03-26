using System;
using Actors.Common;
using Actors.Projectile.Scripts;
using TMPro;
using UnityEngine;

namespace Actors.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public static Action<int, int> OnHealthChanged; // current, max
        public static Action OnPlayerDied;
        public TextMeshProUGUI playerHealthText;

        private void Awake()
        {
            var stats = GetComponent<Stats>();
            MaxHealth = Mathf.RoundToInt(stats.MaxHealth.Value);
            CurrentHealth = MaxHealth;
            SetHealthText(CurrentHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            SetHealthText(CurrentHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth == 0)
                OnPlayerDied?.Invoke();
        }

        private void SetHealthText(int health)
        {
            playerHealthText.text = "Player health: " + health;
        }
    }
}