using UnityEngine;
using UnityEngine.AI;

namespace GameObjects.Enemy.Common.Scripts.Behaviour
{
    public class EnemyPathfinder : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Rigidbody2D _rb;
        private Transform _player;
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

            _agent.SetDestination(_player.position);
            _agent.nextPosition = _rb.position;

            return new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.y).normalized;
        }

        public void Stop()
        {
            _agent.enabled = false;
            _rb.linearVelocity = Vector2.zero;
        }

        public void Resume()
        {
            _agent.enabled = true;
        }

        public void SetStoppingDistance(float distance)
        {
            _agent.stoppingDistance = distance;
        }
    }
}
