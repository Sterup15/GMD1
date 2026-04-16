using Actors.Player.Scripts.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI healthText;

        private void OnEnable()  => PlayerHealth.OnHealthChanged += OnHealthChanged;
        private void OnDisable() => PlayerHealth.OnHealthChanged -= OnHealthChanged;

        private void OnHealthChanged(int current, int max)
        {
            slider.value = (float)current / max;
            healthText.text = $"{current}/{max}";
        }
    }
}