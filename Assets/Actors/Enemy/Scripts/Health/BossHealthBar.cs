using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Actors.Enemy.Scripts.Health
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text nameText;

        private EnemyHealth _enemyHealth;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Initialize(EnemyHealth health, string bossName)
        {
            if (_enemyHealth != null)
                _enemyHealth.OnHealthChanged -= OnHealthChanged;

            _enemyHealth = health;
            _enemyHealth.OnHealthChanged += OnHealthChanged;
            slider.value = 1f;

            if (nameText != null)
                nameText.text = bossName;

            gameObject.SetActive(true);
        }

        private void OnHealthChanged(int current, int max)
        {
            slider.value = (float)current / max;

            if (current == 0)
            {
                _enemyHealth.OnHealthChanged -= OnHealthChanged;
                _enemyHealth = null;
                gameObject.SetActive(false);
            }
        }
    }
}
