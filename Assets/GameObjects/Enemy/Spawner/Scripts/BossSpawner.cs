using System.Collections.Generic;
using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Utils;
using GameObjects.Enemy.Common.Scripts;
using GameObjects.Enemy.Common.Spawner;
using UnityEngine;

namespace GameObjects.Enemy.Spawner.Scripts
{
    // Spawns a boss when the boss timer runs out and restarts the timer when it dies. Everything else reacts to
    // GlobalEvents.OnBossSpawned / OnBossDefeated (waves pause, enemies retreat, music, camera, boss health bar)
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private BossTimer bossTimer;
        [Tooltip("Opened around the player when the boss spawns. Without one the fight happens wherever the player is")]
        [SerializeField] private BossArena arena;
        [SerializeField] private List<BossSpawnConfig> bossTypes = new();
        [Tooltip("Distance from the player the boss appears at")]
        [SerializeField] private float spawnRadius = 12f;
        [Tooltip("Extra room around each player's screen that counts as in view")]
        [SerializeField, Min(0f)] private float viewMargin = 1f;
        [SerializeField] private LayerMask obstacleLayer = 1 << 4;
        [Tooltip("Extra boss health for every player after the first (0.6 = +60% each)")]
        [SerializeField, Min(0f)] private float healthPerExtraPlayer = 0.6f;

        private Transform _player;
        private UnityEngine.Camera _camera;

        private void Start()
        {
            // The host's player; the boss arena rework spawns it relative to the arena instead
            _player = Players.Local;
            if (_player == null) Debug.LogWarning("BossSpawner: No local player registered.");
            _camera = UnityEngine.Camera.main;

            if (bossTimer != null)
                bossTimer.OnTimerComplete += SpawnBoss;
            else
                Debug.LogWarning("BossSpawner: No BossTimer assigned.", this);
        }

        private void OnDestroy()
        {
            if (bossTimer != null)
                bossTimer.OnTimerComplete -= SpawnBoss;
        }

        private void SpawnBoss()
        {
            if (_player == null || bossTypes.Count == 0) return;

            var config = bossTypes[Random.Range(0, bossTypes.Count)];
            if (config?.prefab == null) return;

            // The boss must appear even if every try lands in the water
            // The camera fits the arena when it opens, so the view below is the fight's
            Vector2 center = arena != null ? arena.Open(_player.position) : (Vector2)_player.position;
            var viewHalfSize = SpawnPoints.ViewHalfSize(_camera, viewMargin);
            if (!SpawnPoints.TryAround(center, spawnRadius, viewHalfSize, obstacleLayer, out var position))
                position = center + Random.insideUnitCircle.normalized * spawnRadius;

            var boss = Instantiate(config.prefab, position, Quaternion.identity);

            // Set between Instantiate and the boss's first frame, which reads its health in Start
            int extraPlayers = Mathf.Max(0, Players.All.Count - 1);
            if (extraPlayers > 0 && boss.TryGetComponent<Stats>(out var stats))
                stats.MaxHealth.SetBaseValue(stats.MaxHealth.Value * (1f + healthPerExtraPlayer * extraPlayers));
            if (boss.TryGetComponent<GoldDropper>(out var gold))
                gold.SetAmount(Mathf.RoundToInt(config.goldDrop));

            var events = boss.GetComponent<ActorEvents>();
            if (events != null) events.OnDeath += OnBossDeath;
            GlobalEvents.BossSpawned(events, config.name);
        }

        private void OnBossDeath()
        {
            if (arena != null) arena.Close();
            bossTimer.ResetAndStart();
            GlobalEvents.BossDefeated();
        }
    }
}
