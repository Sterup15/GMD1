using UnityEngine;
using UnityEngine.UI;

namespace Actors.Enemy.Scripts.Health
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        private EnemyHealth _enemyHealth;

        private void Awake()
        {
            _enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        private void OnEnable()  { if (_enemyHealth != null) _enemyHealth.OnHealthChanged += OnHealthChanged; }
        private void OnDisable() { if (_enemyHealth != null) _enemyHealth.OnHealthChanged -= OnHealthChanged; }

        private void OnHealthChanged(int current, int max)
        {
            slider.value = (float)current / max;
        }
    }
}