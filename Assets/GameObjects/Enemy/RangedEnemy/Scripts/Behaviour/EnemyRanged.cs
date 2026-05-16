using GameObjects.Common.Stats.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using UnityEngine;

namespace GameObjects.Enemy.RangedEnemy.Scripts.Behaviour
{
    public class EnemyRanged : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float shootRange = 6f;
        [SerializeField] private float fireRate = 1f;

        private Rigidbody2D _rb;
        private Transform _player;
        private Stats _stats;
        private EnemyPathfinder _pathfinder;
        private EnemyRangedMovementState _movementState;
        private Vector2 _moveDirection;

        public Vector2 MoveDirection => _moveDirection;
        public float AttackAnimSpeed => _stats != null ? _stats.FireRate.Value : fireRate;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<Stats>();
            _pathfinder = GetComponent<EnemyPathfinder>();
            _movementState = GetComponent<EnemyRangedMovementState>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("EnemyRanged: No GameObject tagged 'Player' found.");

        }

        private void FixedUpdate()
        {
            if (_player == null) return;

            if (_movementState.IsAttacking)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float range = _stats != null ? _stats.ShootRange.Value : shootRange;
            float dist = Vector2.Distance(transform.position, _player.position);

            if (dist < range)
            {
                _moveDirection = ((Vector2)_player.position - _rb.position).normalized;
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            _moveDirection = _pathfinder.GetSteerDirection();
            float speed = _stats != null ? _stats.MoveSpeed.Value : moveSpeed;
            _rb.linearVelocity = _moveDirection * speed;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_stats == null) _stats = GetComponent<Stats>();
            float range = _stats != null ? _stats.ShootRange.Value : shootRange;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, range);
        }
#endif
    }
}
