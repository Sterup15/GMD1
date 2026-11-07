using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Events;
using GameObjects.Enemy.Common.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using GameObjects.Common.Projectiles.Scripts;
using UnityEngine;

namespace GameObjects.Enemy.TogreBoss.Scripts.Behaviour
{
    public class BossMelee : MonoBehaviour
    {
        [SerializeField] private float lungeSpeed = 5f;
        [SerializeField] private BossHitbox hitbox;

        private Rigidbody2D _rb;
        private EnemyTargetSelector _targetSelector;
        private BossMeleeMovementState _movementState;
        private Stats _stats;
        private ActorEvents _events;

        private Vector2 _moveDirection;
        private Vector2 _lungeDirection;

        public Vector2 MoveDirection => _moveDirection;
        public bool IsMoving
        {
            get
            {
                var player = _targetSelector.Target;
                return player != null && (player.position - transform.position).sqrMagnitude > 0.01f;
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _movementState = GetComponent<BossMeleeMovementState>();
            _stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            _targetSelector = GetComponent<EnemyTargetSelector>();
        }

        private void OnEnable()
        {
            _events.OnWindupComplete += SnapshotLungeDirection;
            _events.OnHitboxOpen    += OpenHitbox;
            _events.OnHitboxClose   += CloseHitbox;
            if (hitbox != null) hitbox.OnHit += HandleHitboxContact;
        }

        private void OnDisable()
        {
            _events.OnWindupComplete -= SnapshotLungeDirection;
            _events.OnHitboxOpen    -= OpenHitbox;
            _events.OnHitboxClose   -= CloseHitbox;
            if (hitbox != null) hitbox.OnHit -= HandleHitboxContact;
        }

        private void Start()
        {
            if (_stats == null)
                Debug.LogError("BossMelee: Missing Stats component.", this);
        }

        private void FixedUpdate()
        {
            if (_stats == null) return;

            switch (_movementState.CurrentMoveState)
            {
                case EnemyMoveStateEnum.Windup:
                    _rb.linearVelocity = Vector2.zero;
                    return;

                case EnemyMoveStateEnum.Attack:
                    _rb.linearVelocity = _lungeDirection * lungeSpeed;
                    return;

                case EnemyMoveStateEnum.Recover:
                case EnemyMoveStateEnum.Die:
                    _rb.linearVelocity = Vector2.zero;
                    return;
            }

            var player = _targetSelector.Target;
            if (player == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toPlayer = (Vector2)player.position - _rb.position;
            float range = _stats.ShootRange.Value;
            if (toPlayer.sqrMagnitude <= range * range)
            {
                _movementState.SetMoveState(EnemyMoveStateEnum.Windup);
                return;
            }

            _moveDirection = toPlayer.normalized;
            _rb.linearVelocity = _moveDirection * _stats.MoveSpeed.Value;
        }

        private void SnapshotLungeDirection()
        {
            var player = _targetSelector.Target;
            if (player == null) return;
            _lungeDirection = ((Vector2)player.position - _rb.position).normalized;
            _moveDirection = _lungeDirection;
        }

        private void OpenHitbox()  => hitbox?.Open();
        private void CloseHitbox() => hitbox?.Close();

        private void HandleHitboxContact(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(Mathf.RoundToInt(_stats.Damage.Value));
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_stats == null) _stats = GetComponent<Stats>();
            if (_stats == null) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _stats.ShootRange.Value);
        }
#endif
    }
}
