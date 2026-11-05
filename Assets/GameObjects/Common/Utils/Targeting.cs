using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Common.Utils
{
    // The one place that finds the nearest enemy. Enemies register themselves (EnemyTarget), so a lookup walks a
    // short list instead of searching the scene by tag, which allocated a new array on every call
    public static class Targeting
    {
        private static readonly List<Transform> Enemies = new();

        public static void AddEnemy(Transform enemy)    => Enemies.Add(enemy);
        public static void RemoveEnemy(Transform enemy) => Enemies.Remove(enemy);

        // Nearest registered enemy, or null when none is within maxRange
        public static Transform NearestEnemy(Vector2 from, float maxRange = float.PositiveInfinity)
        {
            float bestSqr = maxRange * maxRange;
            Transform nearest = null;
            foreach (var enemy in Enemies)
            {
                float sqr = ((Vector2)enemy.position - from).sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                nearest = enemy;
            }
            return nearest;
        }
    }
}
