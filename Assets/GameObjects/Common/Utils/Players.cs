using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Common.Utils
{
    // Every player in the run, registered by their PlayerTarget component. Local is the one this device controls
    // (camera, HUD, upgrade cards); Nearest and All are for things that react to any player (enemies, spawners).
    // In solo there is one player and it is both. A player who is out (dead) stays registered, so the camera and
    // HUD keep them, but enemies stop targeting them
    public static class Players
    {
        private static readonly List<Transform> Registered = new();
        private static readonly List<Transform> Out = new();

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
            Out.Remove(player);
            if (Local == player) Local = null;
        }

        public static void SetOut(Transform player, bool isOut)
        {
            if (!isOut)
                Out.Remove(player);
            else if (!Out.Contains(player))
                Out.Add(player);
        }

        // Registered and not out, so enemies may go after them
        public static bool IsTargetable(Transform player) => Registered.Contains(player) && !Out.Contains(player);

        // Nearest player who isn't out, or null when there is none
        public static Transform Nearest(Vector2 from)
        {
            float bestSqr = float.PositiveInfinity;
            Transform nearest = null;
            foreach (var player in Registered)
            {
                if (Out.Contains(player)) continue;
                float sqr = ((Vector2)player.position - from).sqrMagnitude;
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                nearest = player;
            }
            return nearest;
        }
    }
}
