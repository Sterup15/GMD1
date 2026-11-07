using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Projectiles.Scripts
{
    // Fires a projectile on every ShotFired animation event of its actor
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private TargetModeEnum targetMode;

        private Stats.Scripts.Stats _stats;
        private ActorEvents _events;
        private ITargetProvider _targetProvider;

        private void Awake()
        {
            _stats  = GetComponent<Stats.Scripts.Stats>();
            _events = GetComponent<ActorEvents>();
            if (spawnPoint == null) spawnPoint = transform;

            if (targetMode != TargetModeEnum.Player) return;
            _targetProvider = GetComponent<ITargetProvider>();
            if (_targetProvider == null)
                Debug.LogWarning("ProjectileSpawner: Player target mode needs a target provider (e.g. EnemyTargetSelector).", this);
        }

        private void OnEnable()  => _events.OnShotFired += Fire;
        private void OnDisable() => _events.OnShotFired -= Fire;

        private void Fire()
        {
            Vector2? targetPosition = GetTargetPosition();
            if (targetPosition == null) return;

            Vector2 direction = (targetPosition.Value - (Vector2)spawnPoint.position).normalized;
            Spawn(projectilePrefab, spawnPoint.position, direction, _stats);
            _events.ProjectileFired(spawnPoint.position, direction);
        }

        // Fires one projectile carrying the shooter's Damage, Bounce and Penetration. Also used by talents that
        // add extra projectiles, which doesn't raise ProjectileFired again
        public static void Spawn(GameObject prefab, Vector2 position, Vector2 direction, Stats.Scripts.Stats stats)
        {
            var go = PrefabPool.Spawn(prefab, position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();
            if (stats != null)
            {
                projectile.SetDamage(Mathf.RoundToInt(stats.Damage.Value));
                projectile.SetBounces(Mathf.RoundToInt(stats.Bounce.Value));
                projectile.SetPenetration(Mathf.RoundToInt(stats.Penetration.Value));
            }
            projectile.Launch(direction);
        }

        private Vector2? GetTargetPosition()
        {
            switch (targetMode)
            {
                case TargetModeEnum.Player:
                    var player = _targetProvider?.Target;
                    return player != null ? (Vector2?)player.position : null;

                case TargetModeEnum.NearestEnemy:
                    var enemy = Targeting.NearestEnemy(spawnPoint.position);
                    return enemy != null ? (Vector2?)enemy.position : null;

                default:
                    return null;
            }
        }
    }
}
