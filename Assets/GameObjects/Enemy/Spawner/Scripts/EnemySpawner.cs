using System;
using System.Collections.Generic;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Events;
using GameObjects.Enemy.Common.Scripts;
using GameObjects.Enemy.Common.Spawner;
using GameObjects.Enemy.TogreBoss.Scripts.Health;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameObjects.Enemy.Spawner.Scripts
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

    [Serializable]
    public class BossSpawnConfig
    {
        public string name;
        public GameObject prefab;

        [Header("Base Stats")]
        public float baseHealth = 100f;
        public float baseMoveSpeed = 2f;
        public float baseDamage = 5f;
        public float baseShootRange = 2f;
        public float baseFireRate = 1f;

        [Header("Gold Drop")]
        public float goldDrop = 100f;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<EnemySpawnConfig> enemyTypes = new();
        [SerializeField] private float spawnRadius = 8f;
        [SerializeField] private float baseSpawnInterval = 3f;

        [Header("Spawn Rate Scaling")]
        [Tooltip("X = run time in seconds, Y = multiplier on spawn rate")]
        [SerializeField] private AnimationCurve spawnRateCurve = AnimationCurve.Linear(0f, 1f, 300f, 3f);

        [Header("Minimum Enemies Alive")]
        [Tooltip("X = run time in seconds, Y = minimum number of enemies that must be alive")]
        [SerializeField] private AnimationCurve minEnemiesAliveCurve = AnimationCurve.Linear(0f, 1f, 300f, 30f);

        [Header("Boss")]
        [SerializeField] private BossTimer bossTimer;
        [SerializeField] private List<BossSpawnConfig> bossTypes = new();
        [SerializeField] private BossHealthBar bossHealthBar;

        private float _runTime;
        private float _nextSpawnTime;
        private Transform _player;
        private bool _isBossFightActive;
        private int _aliveEnemyCount;

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("EnemySpawner: No GameObject tagged 'Player' found.");

            if (bossTimer != null)
                bossTimer.OnTimerComplete += OnBossTimerComplete;

            ScheduleNextSpawn();
        }

        private void OnDestroy()
        {
            if (bossTimer != null)
                bossTimer.OnTimerComplete -= OnBossTimerComplete;
        }

        private void Update()
        {
            if (_isBossFightActive) return;

            _runTime += Time.deltaTime;

            if (Time.time >= _nextSpawnTime)
            {
                SpawnEnemy();
                ScheduleNextSpawn();
            }

            int minAlive = Mathf.RoundToInt(minEnemiesAliveCurve.Evaluate(_runTime));
            int deficit = minAlive - _aliveEnemyCount;
            for (int i = 0; i < deficit; i++)
                SpawnEnemy();
        }

        private void OnBossTimerComplete()
        {
            _isBossFightActive = true;
            SpawnBoss();
        }

        private void OnBossDeath()
        {
            _isBossFightActive = false;
            bossTimer.ResetAndStart();
            ScheduleNextSpawn();
            GlobalEvents.BossDefeated();
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

            _aliveEnemyCount++;
            var enemyEvents = enemy.GetComponent<ActorEvents>();
            if (enemyEvents != null)
                enemyEvents.OnDeath += () => _aliveEnemyCount--;

            var stats = enemy.GetComponent<Stats>();
            if (stats != null)
            {
                stats.MaxHealth.SetBaseValue(config.baseHealth * Evaluate(config.healthCurve, _runTime, 1f));
                stats.MoveSpeed.SetBaseValue(config.baseMoveSpeed * Evaluate(config.speedCurve, _runTime, 1f));
                stats.Damage.SetBaseValue(config.baseDamage * Evaluate(config.damageCurve, _runTime, 1f));
                stats.ShootRange.SetBaseValue(config.baseShootRange);
                stats.FireRate.SetBaseValue(config.baseFireRate);
            }

            enemy.GetComponent<GoldDropper>()?.SetAmount(Mathf.RoundToInt(Evaluate(config.goldDropCurve, _runTime, 10f)));
        }

        private void SpawnBoss()
        {
            if (_player == null || bossTypes.Count == 0) return;

            var config = bossTypes[Random.Range(0, bossTypes.Count)];
            if (config?.prefab == null) return;

            var boss = Instantiate(config.prefab, GetSpawnPosition(), Quaternion.identity);

            var stats = boss.GetComponent<Stats>();
            if (stats != null)
            {
                stats.MaxHealth.SetBaseValue(config.baseHealth);
                stats.MoveSpeed.SetBaseValue(config.baseMoveSpeed);
                stats.Damage.SetBaseValue(config.baseDamage);
                stats.ShootRange.SetBaseValue(config.baseShootRange);
                stats.FireRate.SetBaseValue(config.baseFireRate);
            }

            var actorEvents = boss.GetComponent<ActorEvents>();
            if (actorEvents != null)
            {
                boss.GetComponent<GoldDropper>()?.SetAmount(Mathf.RoundToInt(config.goldDrop));
                actorEvents.OnDeath += OnBossDeath;
                bossHealthBar?.Initialize(actorEvents, config.name);
            }

            GlobalEvents.BossSpawned();
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

        [SerializeField] private LayerMask obstacleLayer = 1 << 4;
        private const int MaxSpawnAttempts = 10;

        private Vector2 GetSpawnPosition()
        {
            for (int i = 0; i < MaxSpawnAttempts; i++)
            {
                Vector2 candidate = (Vector2)_player.position + Random.insideUnitCircle.normalized * spawnRadius;
                if (Physics2D.OverlapPoint(candidate, obstacleLayer) == null)
                    return candidate;
            }

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
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireSphere(_player.position, spawnRadius);
        }
#endif
    }
}
