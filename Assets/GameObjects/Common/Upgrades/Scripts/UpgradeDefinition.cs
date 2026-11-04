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
        [Tooltip("Flat amount, or a percentage of the character's base value when percentOfBase is on")]
        public float bonusAmount;
        [Tooltip("If true, bonusAmount is a percentage of the character's base value, so the card scales with each character's stats")]
        public bool percentOfBase;
        [Tooltip("If true, a flat bonusAmount is multiplied by the bonus scale set on UpgradeScreen. Ignored for percentOfBase cards")]
        public bool applyBonusScale = true;
        public UpgradeRarity rarity = UpgradeRarity.Common;
        [Tooltip("Only offered when the player's UpgradeEligibility allows it (e.g. bounce needs projectiles)")]
        public UpgradeRequirementType requirement = UpgradeRequirementType.None;
        [Tooltip("If false, this upgrade is removed from the pool once chosen")]
        public bool canRepeat = true;
    }
}
