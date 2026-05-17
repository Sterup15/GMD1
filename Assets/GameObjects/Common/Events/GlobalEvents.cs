using System;

namespace GameObjects.Common.Events
{
    public static class GlobalEvents
    {
        public static event Action OnBossSpawned;
        public static event Action OnBossDefeated;
        public static event Action OnPlayerDied;
        public static event Action OnPlayerWon;
        public static event Action<int> OnLevelUp;
        public static event Action OnBounceUnlocked;
        public static bool IsBounceUnlocked { get; private set; }

        public static void BossSpawned()      => OnBossSpawned?.Invoke();
        public static void BossDefeated()     => OnBossDefeated?.Invoke();
        public static void PlayerDied()       => OnPlayerDied?.Invoke();
        public static void PlayerWon()        => OnPlayerWon?.Invoke();
        public static void LevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void BounceUnlocked()   { IsBounceUnlocked = true; OnBounceUnlocked?.Invoke(); }
    }
}
