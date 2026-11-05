using System.Collections.Generic;
using GameObjects.Common.Events;
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
        [SerializeField] private List<BossSpawnConfig> bossTypes = new();
        [Tooltip("Distance from the player the boss appears at")]
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private LayerMask obstacleLayer = 1 << 4;

        private Transform _player;

        private void Start()
        {
            // The host's player; the boss arena rework spawns it relative to the arena instead
            _player = Players.Local;
            if (_player == null) Debug.LogWarning("BossSpawner: No local player registered.");

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
            if (!SpawnPoints.TryAround(_player.position, spawnRadius, obstacleLayer, out var position))
                position = (Vector2)_player.position + Random.insideUnitCircle.normalized * spawnRadius;

            var boss = Instantiate(config.prefab, position, Quaternion.identity);
            if (boss.TryGetComponent<GoldDropper>(out var gold))
                gold.SetAmount(Mathf.RoundToInt(config.goldDrop));

            var events = boss.GetComponent<ActorEvents>();
            if (events != null) events.OnDeath += OnBossDeath;
            GlobalEvents.BossSpawned(events, config.name);
        }

        private void OnBossDeath()
        {
            bossTimer.ResetAndStart();
            GlobalEvents.BossDefeated();
        }
    }
}
