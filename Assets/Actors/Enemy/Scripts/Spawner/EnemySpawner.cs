using System;
using System.Collections.Generic;
using Actors.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Actors.Enemy.Scripts.Spawner
{
    [Serializable]
    public class EnemySpawnConfig
    {
        public string name;
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnWeight = 0.5f;

        [Header("Base Stats")]
        public float baseHealth = 3f;
        public float baseMoveSpeed = 3f;
        public float baseDamage = 1f;
        public float baseShootRange = 6f;
        public float baseFireRate = 1f;

        [Header("Difficulty Scaling")]
        [Tooltip("X = run time in seconds, Y = multiplier on base health")]
        public AnimationCurve healthCurve = AnimationCurve.Linear(0f, 1f, 300f, 5f);
        [Tooltip("X = run time in seconds, Y = multiplier on base move speed")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 300f, 2f);
        [Tooltip("X = run time in seconds, Y = multiplier on base damage")]
        public AnimationCurve damageCurve = AnimationCurve.Linear(0f, 1f, 300f, 3f);

        [Header("Gold Drop")]
        [Tooltip("X = run time in seconds, Y = gold dropped")]
        public AnimationCurve goldDropCurve = AnimationCurve.Linear(0f, 10f, 300f, 10f);
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<EnemySpawnConfig> enemyTypes = new();
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private float baseSpawnInterval = 3f;

        [Header("Spawn Rate Scaling")]
        [Tooltip("X = run time in seconds, Y = multiplier on spawn rate")]
        [SerializeField] private AnimationCurve spawnRateCurve = AnimationCurve.Linear(0f, 1f, 300f, 3f);

        private float _runTime;
        private float _nextSpawnTime;
        private Transform _player;

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("EnemySpawner: No GameObject tagged 'Player' found.");

            ScheduleNextSpawn();
        }

        private void Update()
        {
            _runTime += Time.deltaTime;

            if (Time.time >= _nextSpawnTime)
            {
                SpawnEnemy();
                ScheduleNextSpawn();
            }
        }

        private void ScheduleNextSpawn()
        {
            float rate = Mathf.Max(0.1f, spawnRateCurve.Evaluate(_runTime));
            _nextSpawnTime = Time.time + baseSpawnInterval / rate;
        }

        private void SpawnEnemy()
        {
            if (_player == null || enemyTypes.Count == 0) return;

            var config = PickConfig();
            if (config?.prefab == null) return;

            var enemy = Instantiate(config.prefab, GetSpawnPosition(), Quaternion.identity);

            var stats = enemy.GetComponent<Stats>();
            if (stats != null)
            {
                stats.MaxHealth.SetBaseValue(config.baseHealth * Evaluate(config.healthCurve, _runTime, 1f));
                stats.MoveSpeed.SetBaseValue(config.baseMoveSpeed * Evaluate(config.speedCurve, _runTime, 1f));
                stats.Damage.SetBaseValue(config.baseDamage * Evaluate(config.damageCurve, _runTime, 1f));
                stats.ShootRange.SetBaseValue(config.baseShootRange);
                stats.FireRate.SetBaseValue(config.baseFireRate);
            }

            var health = enemy.GetComponent<Actors.Enemy.Scripts.Health.EnemyHealth>();
            health?.SetGoldDrop(Mathf.RoundToInt(Evaluate(config.goldDropCurve, _runTime, 10f)));
        }

        private EnemySpawnConfig PickConfig()
        {
            float total = 0f;
            foreach (var e in enemyTypes) total += e.spawnWeight;

            float roll = Random.value * total;
            float cumulative = 0f;
            foreach (var e in enemyTypes)
            {
                cumulative += e.spawnWeight;
                if (roll <= cumulative) return e;
            }

            return enemyTypes[^1];
        }

        private static float Evaluate(AnimationCurve curve, float time, float fallback) =>
            curve != null && curve.length > 0 ? curve.Evaluate(time) : fallback;

        [SerializeField] private LayerMask obstacleLayer = 1 << 4; // Water layer
        private const int MaxSpawnAttempts = 10;

        private Vector2 GetSpawnPosition()
        {
            for (int i = 0; i < MaxSpawnAttempts; i++)
            {
                Vector2 candidate = (Vector2)_player.position + Random.insideUnitCircle.normalized * spawnRadius;
                if (Physics2D.OverlapPoint(candidate, obstacleLayer) == null)
                    return candidate;
            }

            // Fallback: return last candidate even if obstructed
            return (Vector2)_player.position + Random.insideUnitCircle.normalized * spawnRadius;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) _player = playerObj.transform;
            }

            if (_player == null) return;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(_player.position, spawnRadius);
        }
#endif
    }
}
