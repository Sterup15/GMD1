using GameObjects.Common.Events;
using GameObjects.Player.Shared.Scripts.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.Health.Scripts
{
    // The player's health bar. Listens to the player's own ActorEvents, found by tag when the run starts
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI healthText;

        private ActorEvents _events;

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("HealthBar: No GameObject tagged 'Player' found.", this);
                return;
            }

            _events = player.GetComponent<ActorEvents>();
            _events.OnHealthChanged += OnHealthChanged;
            if (player.TryGetComponent<PlayerHealth>(out var health))
                OnHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        // The player is only found in Start, so the first subscription happens there
        private void OnEnable()
        {
            if (_events != null) _events.OnHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (_events != null) _events.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            slider.value = max > 0 ? (float)current / max : 0f;
            healthText.text = $"{current}/{max}";
        }
    }
}
