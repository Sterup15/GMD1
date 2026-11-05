using UnityEngine;

namespace GameObjects.Common.Armory.Scripts
{
    // Gives the player the Armory's bought upgrades at the start of every run. Uses AddBonus, so card percentages
    // (worked out from BaseValue) and UpgradeScaling stay the same with or without the Armory
    [RequireComponent(typeof(Stats.Scripts.Stats))]
    public class ArmoryBonuses : MonoBehaviour
    {
        // Start, so components that react to stat changes (e.g. PlayerHealth refilling to the new max) are listening
        private void Start()
        {
            var stats = GetComponent<Stats.Scripts.Stats>();
            foreach (var upgrade in Armory.Upgrades)
            {
                int level = Armory.Level(upgrade);
                if (level <= 0) continue;

                var stat = stats.GetStat(upgrade.Stat);
                if (stat != null) stat.AddBonus(upgrade.BonusFor(stat, level));
            }
        }
    }
}
