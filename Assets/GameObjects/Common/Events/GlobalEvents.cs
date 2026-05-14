using System;

namespace GameObjects.Common.Events
{
    public static class GlobalEvents
    {
        public static event Action OnBossSpawned;
        public static event Action OnBossDefeated;
        public static event Action OnPlayerDied;

        public static void BossSpawned()  => OnBossSpawned?.Invoke();
        public static void BossDefeated() => OnBossDefeated?.Invoke();
        public static void PlayerDied()   => OnPlayerDied?.Invoke();
    }
}
