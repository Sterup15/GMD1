using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Common.Utils
{
    // Every player in the run, registered by their PlayerTarget component. Local is the one this device controls
    // (camera, HUD, upgrade cards); Nearest and All are for things that react to any player (enemies, spawners).
    // In solo there is one player and it is both
    public static class Players
    {
        private static readonly List<Transform> Registered = new();

        public static IReadOnlyList<Transform> All => Registered;
        public static Transform Local { get; private set; }

        public static void Add(Transform player, bool isLocal)
        {
            if (Registered.Contains(player)) return;
            Registered.Add(player);
            if (isLocal) Local = player;
        }

        public static void Remove(Transform player)
        {
            Registered.Remove(player);
            if (Local == player) Local = null;
        }

        // Nearest registered player, or null when there is none
        public static Transform Nearest(Vector2 from)
        {
            float bestSqr = float.PositiveInfinity;
            Transform nearest = null;
            foreach (var player in Registered)
            {
                float sqr = ((Vector2)player.position - from).sqrMagnitude;
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                nearest = player;
            }
            return nearest;
        }
    }
}
