using System;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Common.Upgrades.Scripts
{
    // What one upgrade point (the "+1" on a card) is worth for a stat
    [Serializable]
    public struct StatPointValue
    {
        public StatType stat;
        [Tooltip("Flat amount per point, or a percentage of the character's base value when percentOfBase is on")]
        public float perPoint;
        [Tooltip("Scale with the character's own base value, so a slow-firing character doesn't outgrow a fast one")]
        public bool percentOfBase;

        public StatPointValue(StatType stat, float perPoint, bool percentOfBase = false)
        {
            this.stat          = stat;
            this.perPoint      = perPoint;
            this.percentOfBase = percentOfBase;
        }
    }
}
