using System;
using System.Collections.Generic;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using GameObjects.Enemy.Common.Scripts;
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

        [Header("Difficulty Scaling")]
        [Tooltip("X = run time in seconds, Y = multiplier on base health")]
        public AnimationCurve healthCurve = AnimationCurve.Linear(0f, 1f, 300f, 10f);
        [Tooltip("X = run time in seconds, Y = multiplier on base move speed")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 300f, 1.5f);
        [Tooltip("X = run time in seconds, Y = multiplier on base damage")]
        public AnimationCurve damageCurve = AnimationCurve.Linear(0f, 1f, 300f, 1f);

        [Header("Gold Drop")]
        [Tooltip("X = run time in seconds, Y = gold dropped")]
        public AnimationCurve goldDropCurve = AnimationCurve.Linear(0f, 1f, 300f, 1f);

        public void InitializeDefaultCurves()
        {
            if (healthCurve   == null || healthCurve.length   == 0) healthCurve   = CurveUtils.EaseIn(0f, 1f,  300f, 10f);
            if (speedCurve    == null || speedCurve.length    == 0) speedCurve    = CurveUtils.EaseIn(0f, 1f,  300f, 1.5f);
            if (damageCurve   == null || damageCurve.length   == 0) damageCurve   = AnimationCurve.Linear(0f, 1f,  300f, 1f);
            if (goldDropCurve == null || goldDropCurve.length == 0) goldDropCurve = CurveUtils.EaseIn(0f, 1f, 300f, 1f);
        }
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<EnemySpawnConfig> enemyTypes = new();
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private float baseSpawnInterval = 3f;

        [Header("Spawn Rate Scaling")]
        [Tooltip("X = run time in seconds, Y = multiplier on spawn rate")]
        [SerializeField] private AnimationCurve spawnRateCurve;

        [Header("Minimum Enemies Alive")]
        [Tooltip("X = run time in seconds, Y = minimum number of enemies that must be alive")]
        [SerializeField] private AnimationCurve minEnemiesAliveCurve = AnimationCurve.Linear(0f, 1f, 300f, 30f);
        [Tooltip("Max enemies spawned in one frame to reach the minimum, so a big gap is filled over a few frames instead of in one spike")]
        [SerializeField, Min(1)] private int maxTopUpPerFrame = 3;

        private float _runTime;
        private float _nextSpawnTime;
        private Transform _player;
        private bool _isBossFightActive;
        private int _aliveEnemyCount;

        private void Awake()
        {
            foreach (var config in enemyTypes)
                config.InitializeDefaultCurves();

            if (spawnRateCurve == null || spawnRateCurve.length == 0)
                spawnRateCurve = CurveUtils.EaseIn(0f, 1f, 300f, 5f);
        }

        private void Start()
        {
            // One anchor player for now; spawning around every player comes with the spawner rework
            _player = Players.Local;
            if (_player == null) Debug.LogWarning("EnemySpawner: No local player registered.");

            ScheduleNextSpawn();
        }

        // Waves pause while a boss is out. BossSpawner owns the boss; this only listens
        private void OnEnable()
        {
            GlobalEvents.OnBossSpawned  += PauseWaves;
            GlobalEvents.OnBossDefeated += ResumeWaves;
        }

        private void OnDisable()
        {
            GlobalEvents.OnBossSpawned  -= PauseWaves;
            GlobalEvents.OnBossDefeated -= ResumeWaves;
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
            int deficit = Mathf.Min(minAlive - _aliveEnemyCount, maxTopUpPerFrame);
            for (int i = 0; i < deficit; i++)
                SpawnEnemy();
        }

        private void PauseWaves()
        {
            _isBossFightActive = true;
            // Regular enemies leave without dying, so OnDeath never counts them down
            _aliveEnemyCount = 0;
        }

        private void ResumeWaves()
        {
            _isBossFightActive = false;
            ScheduleNextSpawn();
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

            // No dry spot this time: try again next tick instead
            if (!SpawnPoints.TryAround(_player.position, spawnRadius, obstacleLayer, out var position)) return;

            var enemy = Instantiate(config.prefab, position, Quaternion.identity);

            _aliveEnemyCount++;
            var enemyEvents = enemy.GetComponent<ActorEvents>();
            if (enemyEvents != null)
                enemyEvents.OnDeath += () => _aliveEnemyCount--;

            var stats = enemy.GetComponent<Stats>();
            if (stats != null)
            {
                stats.MaxHealth.SetBaseValue(stats.MaxHealth.Value * Evaluate(config.healthCurve, _runTime, 1f));
                stats.MoveSpeed.SetBaseValue(stats.MoveSpeed.Value * Evaluate(config.speedCurve, _runTime, 1f));
                stats.Damage.SetBaseValue(stats.Damage.Value * Evaluate(config.damageCurve, _runTime, 1f));
            }

            enemy.GetComponent<GoldDropper>()?.SetAmount(Mathf.RoundToInt(Evaluate(config.goldDropCurve, _runTime, 10f)));
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var config in enemyTypes)
                config.InitializeDefaultCurves();
            
            if (spawnRateCurve == null || spawnRateCurve.length == 0)
                spawnRateCurve = CurveUtils.EaseIn(0f, 1f, 300f, 5f);
            
        }

        private void OnDrawGizmosSelected()
        {
            if (_player == null) _player = Players.Local;
            if (_player == null) return;
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireSphere(_player.position, spawnRadius);
        }
#endif
    }
}
