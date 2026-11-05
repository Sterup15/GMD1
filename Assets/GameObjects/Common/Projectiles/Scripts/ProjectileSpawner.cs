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
        private Transform _player;

        private void Awake()
        {
            _stats  = GetComponent<Stats.Scripts.Stats>();
            _events = GetComponent<ActorEvents>();
            if (spawnPoint == null) spawnPoint = transform;
        }

        private void Start()
        {
            if (targetMode != TargetModeEnum.Player) return;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        private void OnEnable()  => _events.OnShotFired += Fire;
        private void OnDisable() => _events.OnShotFired -= Fire;

        private void Fire()
        {
            Vector2? targetPosition = GetTargetPosition();
            if (targetPosition == null) return;

            Vector2 direction = (targetPosition.Value - (Vector2)spawnPoint.position).normalized;
            var go = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();
            if (_stats != null)
            {
                projectile.SetDamage(Mathf.RoundToInt(_stats.Damage.Value));
                projectile.SetBounces(Mathf.RoundToInt(_stats.Bounce.Value));
                projectile.SetPenetration(Mathf.RoundToInt(_stats.Penetration.Value));
            }
            projectile.Launch(direction);
        }

        private Vector2? GetTargetPosition()
        {
            switch (targetMode)
            {
                case TargetModeEnum.Player:
                    return _player != null ? (Vector2?)_player.position : null;

                case TargetModeEnum.NearestEnemy:
                    var enemy = Targeting.NearestEnemy(spawnPoint.position);
                    return enemy != null ? (Vector2?)enemy.position : null;

                default:
                    return null;
            }
        }
    }
}
