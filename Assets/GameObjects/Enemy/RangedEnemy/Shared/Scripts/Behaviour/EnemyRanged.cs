using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using UnityEngine;

namespace GameObjects.Enemy.RangedEnemy.Shared.Scripts.Behaviour
{
    public class EnemyRanged : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Transform _player;
        private Stats _stats;
        private EnemyRangedMovementState _movementState;
        private ActorEvents _events;
        private Vector2 _moveDirection;

        public Vector2 MoveDirection => _moveDirection;
        public float AttackAnimSpeed => _stats.FireRate.Value;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<Stats>();
            _movementState = GetComponent<EnemyRangedMovementState>();
            _events = GetComponent<ActorEvents>();
        }

        private void OnEnable()  => _events.OnRetreat += HandleRetreat;
        private void OnDisable() => _events.OnRetreat -= HandleRetreat;

        // EnemyRetreat drives movement from here on
        private void HandleRetreat(Vector2 _) => enabled = false;

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

            Vector2 toPlayer = (Vector2)_player.position - _rb.position;
            _moveDirection = toPlayer.normalized;

            float range = _stats.ShootRange.Value;
            _rb.linearVelocity = toPlayer.sqrMagnitude < range * range
                ? Vector2.zero
                : _moveDirection * _stats.MoveSpeed.Value;
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
