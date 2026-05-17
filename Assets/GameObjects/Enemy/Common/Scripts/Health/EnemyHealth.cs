using GameObjects.Common.DamageNumber.Scripts;
using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Events;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Enemy.Common.Scripts.Health
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Slider slider;

        private ActorEvents _events;
        private int _currentHealth;
        private int _maxHealth;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()  => _events.OnDeathComplete += FinaliseDeath;
        private void OnDisable() => _events.OnDeathComplete -= FinaliseDeath;

        private void Start()
        {
            var stats = GetComponent<Stats>();
            if (stats == null)
                Debug.LogError("EnemyHealth: Missing Stats component.", this);

            _maxHealth = stats != null ? Mathf.RoundToInt(stats.MaxHealth.Value) : 0;
            _currentHealth = _maxHealth;
            UpdateSlider();
            _events.HealthChanged(_currentHealth, _maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            UpdateSlider();
            _events.HealthChanged(_currentHealth, _maxHealth);
            SpawnDamageNumber(amount);

            if (_currentHealth > 0)
                _events.Hit();

            if (_currentHealth == 0)
            {
                if (_events.HasDeathStartedListeners)
                    _events.DeathStarted();
                else
                    FinaliseDeath();
            }
        }

        public void FinaliseDeath()
        {
            _events.Death();
            Destroy(gameObject);
        }

        private void UpdateSlider()
        {
            if (slider != null)
                slider.value = _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;
        }

        private void SpawnDamageNumber(int amount)
        {
            var go = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity);
            go.GetComponent<DamageNumber>().Setup(amount);
        }
    }
}
