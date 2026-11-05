using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Upgrades.Scripts;
using UnityEngine;

namespace GameObjects.Common.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "BulletHell/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public StatType statType;
        [Tooltip("The +N on the card. What one point is worth per stat is set in UpgradeScreen's point values")]
        [Range(1, 4)] public int points = 1;
        public UpgradeRarity rarity = UpgradeRarity.Common;
        [Tooltip("Only offered when the player's UpgradeEligibility allows it (e.g. bounce needs projectiles)")]
        public UpgradeRequirementType requirement = UpgradeRequirementType.None;
        [Tooltip("If false, this upgrade is removed from the pool once chosen")]
        public bool canRepeat = true;
    }
}
