using GameObjects.Player.Scripts.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.Health.Scripts
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI healthText;

        private void OnEnable()  => PlayerHealth.OnHealthChanged += OnHealthChanged;
        private void OnDisable() => PlayerHealth.OnHealthChanged -= OnHealthChanged;

        private void Start()
        {
            var playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
                OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnHealthChanged(int current, int max)
        {
            slider.value = (float)current / max;
            healthText.text = $"{current}/{max}";
        }
    }
}