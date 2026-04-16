using Actors.Common;
using Actors.Player.Scripts.Health;
using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class EnemyMelee : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float damageCooldown = 1f;

        private Rigidbody2D rb;
        private Transform player;
        private EnemyMeleeMovementState meleeMovementState;
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
            stats = GetComponent<Stats>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EnemyMeleeMelee: No GameObject tagged 'Player' found.");
        }

        private void FixedUpdate()
        {
            if (player == null) return;

            moveDirection = (player.position - transform.position).normalized;
            float speed = stats != null ? stats.MoveSpeed.Value : moveSpeed;
            rb.linearVelocity = moveDirection * speed;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageTime) return;
            if (!other.CompareTag("Player")) return;

            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                int dmg = stats != null ? Mathf.RoundToInt(stats.Damage.Value) : damage;
                health.TakeDamage(dmg);
                nextDamageTime = Time.time + damageCooldown;
                meleeMovementState.SetMoveState(EnemyMoveStateEnum.Attack);
            }
        }
    }
}