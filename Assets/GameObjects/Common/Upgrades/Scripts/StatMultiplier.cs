using System;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Common.Upgrades.Scripts
{
    [Serializable]
    public struct StatMultiplier
    {
        public StatType stat;
        [Tooltip("1 = normal, 0.5 = half as much from every card of this stat, 2 = twice as much")]
        [Min(0f)] public float multiplier;
    }
}
