using Actors.Common;
using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class EnemyRanged : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float shootRange = 6f;
        [SerializeField] private float fireRate = 1f;

        private Rigidbody2D _rb;
        private Transform _player;
        private Stats _stats;
        private ProjectileSpawner _spawner;
        private Vector2 _moveDirection;

        public Vector2 MoveDirection => _moveDirection;
        public bool IsMoving { get; private set; }
        public bool IsAttacking { get; private set; }
        public float AttackAnimSpeed => _stats != null ? _stats.FireRate.Value : fireRate;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<Stats>();
            _spawner = GetComponent<ProjectileSpawner>();
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

            float range = _stats != null ? _stats.ShootRange.Value : shootRange;
            float dist = Vector2.Distance(transform.position, _player.position);

            _moveDirection = (_player.position - transform.position).normalized;

            if (dist > range)
            {
                float speed = _stats != null ? _stats.MoveSpeed.Value : moveSpeed;
                _rb.linearVelocity = _moveDirection * speed;
                IsMoving = true;
                IsAttacking = false;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                IsMoving = false;
                IsAttacking = true;
            }
        }

        // Called via Animation Event on the attack clip
        public void OnShotFired()
        {
            _spawner.Fire();
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