using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;

        private int currentHealth;
        private EnemyMovementState movementState;

        private void Awake()
        {
            currentHealth = maxHealth;
            movementState = GetComponent<EnemyMovementState>();
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= amount;

            if (currentHealth <= 0)
                Destroy(gameObject);
        }
    }
}