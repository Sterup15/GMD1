using System;
using TMPro;
using UnityEngine;

namespace Actors.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 5;

        public int CurrentHealth { get; private set; }
        public static Action<int, int> OnHealthChanged; // current, max
        public static Action OnPlayerDied;
        public TextMeshProUGUI playerHealthText;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            SetHealthText(CurrentHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            SetHealthText(CurrentHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
                OnPlayerDied?.Invoke();
        }

        private void SetHealthText(int health)
        {
            playerHealthText.text = "Player health: " + health;
        }
    }
}