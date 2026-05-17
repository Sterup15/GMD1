using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Common.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "BulletHell/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public StatType statType;
        public float bonusAmount;
        [Tooltip("If true, bonusAmount is multiplied by the bonus scale set on UpgradeScreen")]
        public bool applyBonusScale = true;
        public UpgradeRarity rarity = UpgradeRarity.Common;
        [Tooltip("If false, this upgrade is removed from the pool once chosen")]
        public bool canRepeat = true;
    }
}
