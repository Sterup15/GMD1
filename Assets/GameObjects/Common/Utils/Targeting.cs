using UnityEngine;

namespace GameObjects.Common.Utils
{
    public static class Targeting
    {
        // Nearest GameObject with the tag, or null when none is within maxRange
        public static GameObject Nearest(string tag, Vector2 from, float maxRange = float.PositiveInfinity)
        {
            float bestSqr = maxRange * maxRange;
            GameObject nearest = null;

            foreach (var candidate in GameObject.FindGameObjectsWithTag(tag))
            {
                float sqr = ((Vector2)candidate.transform.position - from).sqrMagnitude;
                if (sqr > bestSqr) continue;

                bestSqr = sqr;
                nearest = candidate;
            }

            return nearest;
        }
    }
}
