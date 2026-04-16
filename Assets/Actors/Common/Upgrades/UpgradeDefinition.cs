using UnityEngine;

namespace Actors.Common.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "BulletHell/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public StatType statType;
        public float bonusAmount;
        [Tooltip("If false, this upgrade is removed from the pool once chosen")]
        public bool canRepeat = true;
    }
}
