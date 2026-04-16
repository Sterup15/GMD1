using System;
using Actors.Common;
using Actors.Common.Gold;
using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Enemy.Scripts.Health
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private GameObject goldPickupPrefab;

        public event Action<int, int> OnHealthChanged; // current, max

        private int _currentHealth;
        private int _goldDrop;
        private EnemyMeleeMovementState meleeMovementState;

        public void SetGoldDrop(int amount) => _goldDrop = amount;

        private void Awake()
        {
            meleeMovementState = GetComponent<EnemyMeleeMovementState>();
        }

        private void Start()
        {
            var stats = GetComponent<Stats>();
            if (stats != null)
                maxHealth = Mathf.RoundToInt(stats.MaxHealth.Value);

            _currentHealth = maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            SpawnDamageNumber(amount);

            if (_currentHealth == 0)
            {
                DropGold();
                Destroy(gameObject);
            }
        }

        private void DropGold()
        {
            if (goldPickupPrefab == null || _goldDrop <= 0) return;

            var go = Instantiate(goldPickupPrefab, transform.position, Quaternion.identity);
            go.GetComponent<GoldPickup>().SetAmount(_goldDrop);
        }

        private void SpawnDamageNumber(int amount)
        {
            var go = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity);
            go.GetComponent<DamageNumber>().Setup(amount);
        }
    }
}