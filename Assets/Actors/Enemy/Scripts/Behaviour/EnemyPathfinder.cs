using Actors.Common;
using UnityEngine;
using UnityEngine.AI;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class EnemyPathfinder : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Rigidbody2D _rb;
        private Transform _player;
        private Stat _shootRange;

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

            var stats = GetComponent<Stats>();
            _shootRange = stats?.ShootRange;
            if (_shootRange != null)
            {
                _agent.stoppingDistance = _shootRange.Value;
                _shootRange.OnValueChanged += () => _agent.stoppingDistance = _shootRange.Value;
            }

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("EnemyPathfinder: No GameObject tagged 'Player' found.");
        }

        public Vector2 GetSteerDirection()
        {
            if (_player == null) return Vector2.zero;

            _agent.SetDestination(_player.position);
            _agent.nextPosition = _rb.position;

            return new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.y).normalized;
        }
    }
}
