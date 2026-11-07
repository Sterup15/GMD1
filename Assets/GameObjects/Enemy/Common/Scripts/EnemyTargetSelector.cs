using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts
{
    // Picks which player this enemy goes after: the nearest one still in the run. It only looks again every few
    // seconds and switches only to a clearly closer player, so enemies don't flip between two players standing
    // side by side. A player who leaves or is out is dropped at once
    public class EnemyTargetSelector : MonoBehaviour
    {
        [Tooltip("Seconds between checks for a closer player")]
        [SerializeField] private float recheckInterval = 1.5f;
        [Tooltip("How much closer another player must be to switch to them (0.25 = 25% closer)")]
        [Range(0f, 1f)]
        [SerializeField] private float switchMargin = 0.25f;

        private Transform _target;
        private float _nextRecheck;

        // The player to go after, or null when no player is left
        public Transform Target
        {
            get
            {
                if (_target == null || !Players.IsTargetable(_target))
                    _target = Players.Nearest(transform.position);
                return _target;
            }
        }

        // Spread the checks so a wave spawned together doesn't recheck in the same frame
        private void OnEnable() => _nextRecheck = Time.time + Random.Range(0f, recheckInterval);

        private void Update()
        {
            if (Time.time < _nextRecheck) return;
            _nextRecheck = Time.time + recheckInterval;

            var current = Target;
            if (current == null) return;

            Vector2 position = transform.position;
            var nearest = Players.Nearest(position);
            if (nearest == current) return;

            float keep = 1f - switchMargin;
            float currentSqr = ((Vector2)current.position - position).sqrMagnitude;
            float nearestSqr = ((Vector2)nearest.position - position).sqrMagnitude;
            if (nearestSqr < currentSqr * keep * keep)
                _target = nearest;
        }
    }
}
