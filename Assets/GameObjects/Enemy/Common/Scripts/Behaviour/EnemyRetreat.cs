using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts.Behaviour
{
    // Makes a regular enemy run off screen when the boss spawns, then removes it
    public class EnemyRetreat : MonoBehaviour
    {
        [Tooltip("Multiplier on MoveSpeed while running off screen")]
        [SerializeField] private float speedMultiplier = 1.5f;
        [Tooltip("World units past the screen edge before the enemy is removed")]
        [SerializeField] private float offscreenMargin = 1f;
        [Tooltip("Fallback: seconds before the enemy is removed even if it is still on screen")]
        [SerializeField] private float maxRetreatTime = 6f;

        private Rigidbody2D _rb;
        private Stats _stats;
        private EnemyPathfinder _pathfinder;
        private ActorEvents _events;
        private UnityEngine.Camera _camera;
        private Vector2 _direction;
        private bool _isRetreating;

        private void Awake()
        {
            _rb         = GetComponent<Rigidbody2D>();
            _stats      = GetComponent<Stats>();
            _pathfinder = GetComponent<EnemyPathfinder>();
            _events     = GetComponent<ActorEvents>();
        }

        private void OnEnable()  => GlobalEvents.OnBossSpawned += StartRetreat;
        private void OnDisable() => GlobalEvents.OnBossSpawned -= StartRetreat;

        private void StartRetreat()
        {
            _camera = UnityEngine.Camera.main;
            Vector2 center = _camera != null ? (Vector2)_camera.transform.position : Vector2.zero;
            _direction = (_rb.position - center).normalized;
            if (_direction == Vector2.zero) _direction = Vector2.right;

            _isRetreating = true;
            _pathfinder.Stop();

            // Disabling the colliders stops the enemy taking or dealing damage and lets it pass through water on the way out
            foreach (var col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            // Untagged so the player's auto-aim ignores it and targets the boss
            gameObject.tag = "Untagged";

            Destroy(gameObject, maxRetreatTime);
            _events.Retreat(_direction);
        }

        private void FixedUpdate()
        {
            if (!_isRetreating) return;

            _rb.linearVelocity = _direction * (_stats.MoveSpeed.Value * speedMultiplier);
            if (!IsOffscreen()) return;

            _isRetreating = false;
            Destroy(gameObject);
        }

        private bool IsOffscreen()
        {
            if (_camera == null) return false;

            Vector2 offset   = _rb.position - (Vector2)_camera.transform.position;
            float halfHeight = _camera.orthographicSize + offscreenMargin;
            float halfWidth  = _camera.orthographicSize * _camera.aspect + offscreenMargin;
            return Mathf.Abs(offset.x) > halfWidth || Mathf.Abs(offset.y) > halfHeight;
        }
    }
}
