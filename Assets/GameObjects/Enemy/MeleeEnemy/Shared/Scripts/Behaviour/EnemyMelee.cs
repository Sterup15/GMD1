using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Enemy.Common.Scripts;
using UnityEngine;

namespace GameObjects.Enemy.MeleeEnemy.Shared.Scripts.Behaviour
{
    public class EnemyMelee : MonoBehaviour
    {
        [SerializeField] private float damageCooldown = 1f;

        private Rigidbody2D rb;
        private EnemyTargetSelector targetSelector;
        private EnemyMeleeMovementState meleeMovementState;
        private Stats stats;
        private ActorEvents events;
        private float nextDamageTime;
        private Vector2 moveDirection;

        public Vector2 MoveDirection => moveDirection;
        public bool IsMoving
        {
            get
            {
                var player = targetSelector.Target;
                return player != null && (player.position - transform.position).sqrMagnitude > 0.01f;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            meleeMovementState = GetComponent<EnemyMeleeMovementState>();
            stats = GetComponent<Stats>();
            events = GetComponent<ActorEvents>();
            targetSelector = GetComponent<EnemyTargetSelector>();
        }

        private void OnEnable()  => events.OnRetreat += HandleRetreat;
        private void OnDisable() => events.OnRetreat -= HandleRetreat;

        // EnemyRetreat drives movement from here on
        private void HandleRetreat(Vector2 _) => enabled = false;

        private void FixedUpdate()
        {
            var player = targetSelector.Target;
            if (player == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            moveDirection = ((Vector2)player.position - rb.position).normalized;
            rb.linearVelocity = moveDirection * stats.MoveSpeed.Value;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageTime) return;
            if (!other.CompareTag("Player")) return;

            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(Mathf.RoundToInt(stats.Damage.Value));
                nextDamageTime = Time.time + damageCooldown;
                meleeMovementState.SetMoveState(EnemyMoveStateEnum.Attack);
            }
        }
    }
}
