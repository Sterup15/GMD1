using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Enemy.Scripts.Health
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private EnemyHealthBar healthBar;
        [SerializeField] private GameObject damageNumberPrefab;

        private int _currentHealth;
        private EnemyMovementState movementState;

        private void Awake()
        {
            _currentHealth = maxHealth;
            movementState = GetComponent<EnemyMovementState>();
            healthBar.SetHealth(_currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            healthBar.SetHealth(_currentHealth, maxHealth);
            SpawnDamageNumber(amount);
            movementState.SetMoveState(EnemyMoveStateEnum.Hit);

            if (_currentHealth == 0)
                Destroy(gameObject);
        }

        private void SpawnDamageNumber(int amount)
        {
            var go = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity);
            go.GetComponent<DamageNumber>().Setup(amount);
        }
    }
}