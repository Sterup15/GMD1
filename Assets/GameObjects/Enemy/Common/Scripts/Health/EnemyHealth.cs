using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts.Health
{
    // Health and death only. The health bar (EnemyHealthBar) and damage numbers (DamageNumberSpawner)
    // are separate components listening to OnHealthChanged and OnDamaged
    public class EnemyHealth : MonoBehaviour, IDamageable
    {

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
            _events.HealthChanged(_currentHealth, _maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _events.HealthChanged(_currentHealth, _maxHealth);
            _events.Damaged(amount);

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
    }
}
