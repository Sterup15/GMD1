using UnityEngine;
using UnityEngine.AI;

namespace GameObjects.Enemy.Common.Scripts.Behaviour
{
    public class EnemyPathfinder : MonoBehaviour
    {
        [Tooltip("Min distance the player must move before the path is recalculated")]
        [SerializeField] private float minRepathDistance = 0.5f;
        [Tooltip("Extra repath distance per unit between enemy and player, so far away enemies repath less often")]
        [SerializeField] private float repathDistanceFactor = 0.1f;

        private NavMeshAgent _agent;
        private Rigidbody2D _rb;
        private Transform _player;
        private Vector2 _lastDestination;
        private bool _hasDestination;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _agent = GetComponentInChildren<NavMeshAgent>();
        }

        private void Start()
        {
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.stoppingDistance = 0f;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("EnemyPathfinder: No GameObject tagged 'Player' found.");
        }

        public Vector2 GetSteerDirection()
        {
            if (_player == null || !_agent.enabled) return Vector2.zero;

            // A new destination triggers a full path search, so only send one when the player has moved far enough
            Vector2 target = _player.position;
            if (!_hasDestination || NeedsRepath(target))
            {
                _agent.SetDestination(target);
                _lastDestination = target;
                _hasDestination = true;
            }

            _agent.nextPosition = _rb.position;

            return new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.y).normalized;
        }

        private bool NeedsRepath(Vector2 target)
        {
            float distSqr = (target - _rb.position).sqrMagnitude;
            float thresholdSqr = Mathf.Max(minRepathDistance * minRepathDistance,
                                           distSqr * repathDistanceFactor * repathDistanceFactor);
            return (target - _lastDestination).sqrMagnitude > thresholdSqr;
        }

        public void Stop()
        {
            _agent.enabled = false;
            _rb.linearVelocity = Vector2.zero;
        }

        public void Resume()
        {
            _agent.enabled = true;
            // Disabling the agent clears its path
            _hasDestination = false;
        }

        public void SetStoppingDistance(float distance)
        {
            _agent.stoppingDistance = distance;
        }
    }
}
