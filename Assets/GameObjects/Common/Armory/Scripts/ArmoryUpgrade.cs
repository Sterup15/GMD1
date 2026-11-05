using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Common.Armory.Scripts
{
    // One permanent upgrade sold in the Armory for Glory. It applies to every character at the start of each run.
    // Assets live in Common/Armory/Resources/Armory, so a new upgrade is a new asset
    [CreateAssetMenu(fileName = "ArmoryUpgrade", menuName = "BulletHell/Armory Upgrade")]
    public class ArmoryUpgrade : ScriptableObject
    {
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;
        [Tooltip("Position in the Armory list, lowest first")]
        [SerializeField] private int order;

        [Header("Effect")]
        [SerializeField] private StatType stat;
        [Tooltip("Flat amount per level, or a percentage of the character's base value when percentOfBase is on")]
        [SerializeField] private float perLevel = 1f;
        [SerializeField] private bool percentOfBase;
        [SerializeField, Min(1)] private int maxLevel = 5;

        [Header("Cost")]
        [Tooltip("Glory for the first level")]
        [SerializeField, Min(0)] private int baseCost = 100;
        [Tooltip("Extra Glory for each level after the first")]
        [SerializeField, Min(0)] private int costPerLevel = 100;
        [Tooltip("Each level costs this many times the one before (1 = no growth, 1.3 = 30% more every level)")]
        [SerializeField, Min(1f)] private float costGrowth = 1f;

        public string DisplayName => displayName;
        public string Description => description;
        public int Order          => order;
        public StatType Stat      => stat;
        public int MaxLevel       => maxLevel;

        // Glory to buy the given level (1 = the first), rounded to 5 so prices read cleanly
        public int CostOf(int level)
        {
            float cost = baseCost * Mathf.Pow(costGrowth, level - 1) + costPerLevel * (level - 1);
            return Mathf.RoundToInt(cost / 5f) * 5;
        }

        public float BonusFor(Stat target, int level)
        {
            float perLevelValue = percentOfBase ? target.BaseValue * perLevel / 100f : perLevel;
            return perLevelValue * level;
        }
    }
}
