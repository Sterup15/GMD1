using GameObjects.Common.Stats.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using GameObjects.Player.Scripts.Health;
using UnityEngine;

namespace GameObjects.Enemy.MeleeEnemy.Scripts.Behaviour
{
    public class EnemyMelee : MonoBehaviour
    {
        [SerializeField] private float damageCooldown = 1f;

        private Rigidbody2D rb;
        private Transform player;
        private EnemyMeleeMovementState meleeMovementState;
        private EnemyPathfinder pathfinder;
        private Stats stats;
        private float nextDamageTime;
        private Vector2 moveDirection;

        public Vector2 MoveDirection => moveDirection;
        public bool IsMoving => player != null && (player.position - transform.position).sqrMagnitude > 0.01f;
        public bool IsAttacking => false;
        public float AttackAnimSpeed => 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            meleeMovementState = GetComponent<EnemyMeleeMovementState>();
            pathfinder = GetComponent<EnemyPathfinder>();
            stats = GetComponent<Stats>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EnemyMelee: No GameObject tagged 'Player' found.");
        }

        private void FixedUpdate()
        {
            if (player == null) return;

            moveDirection = pathfinder.GetSteerDirection();
            rb.linearVelocity = moveDirection * stats.MoveSpeed.Value;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageTime) return;
            if (!other.CompareTag("Player")) return;

            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(Mathf.RoundToInt(stats.Damage.Value));
                nextDamageTime = Time.time + damageCooldown;
                meleeMovementState.SetMoveState(EnemyMoveStateEnum.Attack);
            }
        }
    }
}
