using UnityEngine;

namespace GameObjects.Common.Progression.Scripts
{
    // Per-character mastery: XP earned only by playing that character (converted from run points like Glory),
    // and the level it adds up to. Characters are keyed by their CharacterDefinition asset name.
    // Going from level n to n + 1 costs BaseCost + CostPerLevel * n XP
    public static class Mastery
    {
        public const int MaxLevel = 20;
        private const int BaseCost = 50;
        private const int CostPerLevel = 25;

        public static int Xp(string characterId) => PlayerPrefs.GetInt(Key(characterId), 0);

        public static int Level(string characterId) => LevelFor(Xp(characterId));

        public static void Add(string characterId, int xp)
        {
            if (xp <= 0 || string.IsNullOrEmpty(characterId)) return;
            PlayerPrefs.SetInt(Key(characterId), Xp(characterId) + xp);
            // The web build only writes PlayerPrefs to the browser's storage on Save
            PlayerPrefs.Save();
        }

        public static int LevelFor(int xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= XpToReach(level + 1)) level++;
            return level;
        }

        // Total XP needed to reach a level from zero
        public static int XpToReach(int level)
        {
            int steps = Mathf.Max(0, level - 1);
            return BaseCost * steps + CostPerLevel * steps * (steps + 1) / 2;
        }

        private static string Key(string characterId) => $"mastery.{characterId}.xp";
    }
}
