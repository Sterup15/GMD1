using UnityEngine;

namespace GameObjects.Enemy.Spawner.Scripts
{
    // Picks a spawn spot on a ring around the player that isn't in the water. Enemies walk straight at the
    // player, so one spawned in the water would get stuck there
    public static class SpawnPoints
    {
        private const int MaxAttempts = 30;

        public static bool TryAround(Vector2 center, float radius, LayerMask obstacles, out Vector2 position)
        {
            for (int i = 0; i < MaxAttempts; i++)
            {
                position = center + Random.insideUnitCircle.normalized * radius;
                if (Physics2D.OverlapPoint(position, obstacles) == null)
                    return true;
            }
            position = default;
            return false;
        }
    }
}
