using System.Collections.Generic;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Enemy.Spawner.Scripts
{
    // Picks spawn spots: on a ring around one player, out of the water (enemies walk straight at the player, so one
    // spawned in the water would get stuck there) and out of every player's view, so nothing pops into sight
    public static class SpawnPoints
    {
        private const int MaxAttempts = 30;

        // A random player still in the run, so every player gets the same share of spawns: a group gets pressure
        // together and a player off on their own still gets theirs. Null when nobody is left
        public static Transform RandomAnchor()
        {
            var players = Players.All;
            int targetable = 0;
            foreach (var player in players)
                if (Players.IsTargetable(player)) targetable++;
            if (targetable == 0) return null;

            int pick = Random.Range(0, targetable);
            foreach (var player in players)
            {
                if (!Players.IsTargetable(player)) continue;
                if (pick-- == 0) return player;
            }
            return null;
        }

        // A dry spot on the ring that no player can see. When every dry try is in someone's view, the dry try
        // furthest from all players is used. False only when every try landed in the water
        public static bool TryAround(Vector2 center, float radius, Vector2 viewHalfSize, LayerMask obstacles,
            out Vector2 position)
        {
            var players = Players.All;
            bool foundDry = false;
            float bestDistance = -1f;
            position = default;

            for (int i = 0; i < MaxAttempts; i++)
            {
                Vector2 candidate = center + Random.insideUnitCircle.normalized * radius;
                if (Physics2D.OverlapPoint(candidate, obstacles) != null) continue;

                if (!InAnyView(candidate, players, viewHalfSize))
                {
                    position = candidate;
                    return true;
                }

                float distance = NearestPlayerSqrDistance(candidate, players);
                if (distance <= bestDistance) continue;
                bestDistance = distance;
                position = candidate;
                foundDry = true;
            }
            return foundDry;
        }

        // Half the size of what one screen shows around its player, plus a margin so a spawning sprite isn't half
        // visible. Online, the host will need the widest screen of all players
        public static Vector2 ViewHalfSize(UnityEngine.Camera camera, float margin)
        {
            if (camera == null || !camera.orthographic) return new Vector2(10f, 6f) + Vector2.one * margin;
            float halfHeight = camera.orthographicSize;
            return new Vector2(halfHeight * camera.aspect + margin, halfHeight + margin);
        }

        private static bool InAnyView(Vector2 point, IReadOnlyList<Transform> players, Vector2 viewHalfSize)
        {
            foreach (var player in players)
            {
                Vector2 offset = point - (Vector2)player.position;
                if (Mathf.Abs(offset.x) < viewHalfSize.x && Mathf.Abs(offset.y) < viewHalfSize.y) return true;
            }
            return false;
        }

        private static float NearestPlayerSqrDistance(Vector2 point, IReadOnlyList<Transform> players)
        {
            float nearest = float.PositiveInfinity;
            foreach (var player in players)
                nearest = Mathf.Min(nearest, (point - (Vector2)player.position).sqrMagnitude);
            return nearest;
        }
    }
}
