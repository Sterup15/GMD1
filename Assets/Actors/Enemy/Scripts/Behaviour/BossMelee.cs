using Actors.Common;
using Actors.Player.Scripts.Health;
using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class BossMelee : MonoBehaviour
    {
        [SerializeField] private float lungeSpeed = 5f;
        [SerializeField] private BossHitbox hitbox;

        private Rigidbody2D _rb;
        private Transform _player;
        private EnemyPathfinder _pathfinder;
        private BossMeleeMovementState _movementState;
        private Stats _stats;

        private Vector2 _moveDirection;
        private Vector2 _lungeDirection;

        public Vector2 MoveDirection => _moveDirection;
        public bool IsMoving => _player != null && (_player.position - transform.position).sqrMagnitude > 0.01f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _pathfinder = GetComponent<EnemyPathfinder>();
            _movementState = GetComponent<BossMeleeMovementState>();
            _stats = GetComponent<Stats>();
        }

        private void Start()
        {
            if (_stats == null)
                Debug.LogError("BossMelee: Missing Stats component.", this);

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("BossMelee: No GameObject tagged 'Player' found.");

            if (hitbox != null)
                hitbox.OnHit += HandleHitboxContact;
        }

        private void OnDestroy()
        {
            if (hitbox != null)
                hitbox.OnHit -= HandleHitboxContact;
        }

        private void FixedUpdate()
        {
            if (_player == null || _stats == null) return;

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

            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist <= _stats.ShootRange.Value)
            {
                _movementState.SetMoveState(EnemyMoveStateEnum.Windup);
                return;
            }

            _moveDirection = _pathfinder.GetSteerDirection();
            _rb.linearVelocity = _moveDirection * _stats.MoveSpeed.Value;
        }

        public void OnWindupComplete()
        {
            _lungeDirection = ((Vector2)_player.position - _rb.position).normalized;
            _moveDirection = _lungeDirection;
        }

        public void OpenHitbox() => hitbox?.Open();
        public void CloseHitbox() => hitbox?.Close();

        private void HandleHitboxContact(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent<PlayerHealth>(out var health))
                health.TakeDamage(Mathf.RoundToInt(_stats.Damage.Value));
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
