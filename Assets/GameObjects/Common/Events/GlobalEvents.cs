using System;
using GameObjects.Common.Progression.Scripts;

namespace GameObjects.Common.Events
{
    public static class GlobalEvents
    {
        public static event Action OnBossSpawned;
        // Same moment, for listeners that need the boss itself (e.g. its health bar)
        public static event Action<ActorEvents, string> OnBossSpawnedWithBoss;
        public static event Action OnBossDefeated;
        public static event Action OnPlayerDied;
        public static event Action OnPlayerWon;
        public static event Action<int> OnLevelUp;
        public static event Action OnEnemyKilled;
        // After OnPlayerDied/OnPlayerWon, once the run's points are counted and banked
        public static event Action<RunResult> OnRunEnded;

        public static void BossSpawned(ActorEvents boss, string bossName)
        {
            OnBossSpawned?.Invoke();
            OnBossSpawnedWithBoss?.Invoke(boss, bossName);
        }

        public static void BossDefeated()     => OnBossDefeated?.Invoke();
        public static void PlayerDied()       => OnPlayerDied?.Invoke();
        public static void PlayerWon()        => OnPlayerWon?.Invoke();
        public static void LevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void EnemyKilled()      => OnEnemyKilled?.Invoke();
        public static void RunEnded(RunResult result) => OnRunEnded?.Invoke(result);
    }
}
