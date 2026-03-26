using Actors.Common;
using UnityEngine;

namespace Actors.Projectile.Scripts
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private TargetModeEnum targetMode;

        private Stats _stats;

        private void Awake()
        {
            _stats = GetComponent<Stats>();
        }

        public void Fire()
        {
            Vector2? targetPosition = GetTargetPosition();
            if (targetPosition == null) return;

            Vector2 direction = (targetPosition.Value - (Vector2)spawnPoint.position).normalized;
            var go = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();
            if (_stats != null)
                projectile.SetDamage(Mathf.RoundToInt(_stats.Damage.Value));
            projectile.Launch(direction);
        }

        private Vector2? GetTargetPosition()
        {
            switch (targetMode)
            {
                case TargetModeEnum.Player:
                    var player = GameObject.FindWithTag("Player");
                    return player != null ? (Vector2?)player.transform.position : null;

                case TargetModeEnum.NearestEnemy:
                    return FindNearestTaggedPosition("Enemy");

                default:
                    return null;
            }
        }

        private Vector2? FindNearestTaggedPosition(string tag)
        {
            var enemies = GameObject.FindGameObjectsWithTag(tag);
            if (enemies.Length == 0) return null;

            GameObject nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.SqrMagnitude(enemy.transform.position - spawnPoint.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest != null ? (Vector2?)nearest.transform.position : null;
        }
    }
}