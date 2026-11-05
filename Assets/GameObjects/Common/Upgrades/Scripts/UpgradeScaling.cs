using System.Collections.Generic;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Common.Upgrades.Scripts
{
    // How strongly this character grows from upgrade cards, per stat. It scales only what a card adds: the cards
    // still show the same points and the character's base stats stay as they are. Stats not listed use 1
    public class UpgradeScaling : MonoBehaviour
    {
        [SerializeField] private List<StatMultiplier> multipliers = new();

        public float For(StatType stat)
        {
            foreach (var entry in multipliers)
                if (entry.stat == stat) return entry.multiplier;
            return 1f;
        }
    }
}
