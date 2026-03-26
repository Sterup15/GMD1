using Actors.Player.Scripts.Health;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyFollow : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float damageCooldown = 1f;

        private Rigidbody2D rb;
        private Transform player;
        private EnemyMovementState movementState;
        private float nextDamageTime;
        private Vector2 moveDirection;

        public Vector2 MoveDirection => moveDirection;
        public bool IsMoving => player != null && (player.position - transform.position).sqrMagnitude > 0.01f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            movementState = GetComponent<EnemyMovementState>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EnemyFollow: No GameObject tagged 'Player' found.");
        }

        private void FixedUpdate()
        {
            if (player == null) return;

            moveDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = moveDirection * moveSpeed;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageTime) return;
            if (!other.CompareTag("Player")) return;

            
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
                movementState.SetMoveState(EnemyMoveStateEnum.Hit);
            }
        }
    }
}