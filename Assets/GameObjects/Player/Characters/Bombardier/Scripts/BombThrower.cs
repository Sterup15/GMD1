using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;
using BombProjectile = GameObjects.Player.Characters.Bombardier.Bomb.Scripts.Bomb;

namespace GameObjects.Player.Characters.Bombardier.Scripts
{
    // The Bombardier's basic attack: throws a bomb at the nearest enemy on every ShotFired animation event
    public class BombThrower : MonoBehaviour
    {
        [SerializeField] private BombProjectile bombPrefab;
        [Tooltip("Where the bomb leaves the hand. Defaults to the player's position")]
        [SerializeField] private Transform spawnPoint;

        private Stats _stats;
        private ActorEvents _events;

        private void Awake()
        {
            _stats  = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            if (spawnPoint == null) spawnPoint = transform;
        }

        private void OnEnable()  => _events.OnShotFired += Throw;
        private void OnDisable() => _events.OnShotFired -= Throw;

        private void Throw()
        {
            var target = Targeting.NearestEnemy(spawnPoint.position);
            if (target == null) return;

            var bomb = PrefabPool.Spawn(bombPrefab, spawnPoint.position, Quaternion.identity);
            bomb.Launch(target.position, Mathf.RoundToInt(_stats.Damage.Value), _stats.Area.Value, _events.BombExploded);
        }
    }
}
