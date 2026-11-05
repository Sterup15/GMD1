using System.Collections.Generic;
using GameObjects.Common.Progression.Scripts;
using UnityEngine;

namespace GameObjects.Common.Armory.Scripts
{
    // The bought Armory levels, saved between runs, and buying or refunding them with Glory
    public static class Armory
    {
        private static List<ArmoryUpgrade> _upgrades;

        // Every Armory upgrade, in display order
        public static IReadOnlyList<ArmoryUpgrade> Upgrades
        {
            get
            {
                if (_upgrades != null) return _upgrades;
                _upgrades = new List<ArmoryUpgrade>(Resources.LoadAll<ArmoryUpgrade>("Armory"));
                _upgrades.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : string.CompareOrdinal(a.name, b.name));
                return _upgrades;
            }
        }

        public static int Level(ArmoryUpgrade upgrade) => PlayerPrefs.GetInt(Key(upgrade), 0);

        public static bool IsMaxed(ArmoryUpgrade upgrade) => Level(upgrade) >= upgrade.MaxLevel;

        public static int NextCost(ArmoryUpgrade upgrade) => upgrade.CostOf(Level(upgrade) + 1);

        public static bool TryBuy(ArmoryUpgrade upgrade)
        {
            if (IsMaxed(upgrade) || !Glory.TrySpend(NextCost(upgrade))) return false;
            PlayerPrefs.SetInt(Key(upgrade), Level(upgrade) + 1);
            PlayerPrefs.Save();
            return true;
        }

        // Gives back all the Glory spent in the Armory and resets every upgrade to level 0
        public static int RefundAll()
        {
            int refund = 0;
            foreach (var upgrade in Upgrades)
            {
                int level = Level(upgrade);
                for (int l = 1; l <= level; l++) refund += upgrade.CostOf(l);
                PlayerPrefs.DeleteKey(Key(upgrade));
            }
            Glory.Add(refund);
            PlayerPrefs.Save();
            return refund;
        }

        private static string Key(ArmoryUpgrade upgrade) => $"armory.{upgrade.name}.level";
    }
}
