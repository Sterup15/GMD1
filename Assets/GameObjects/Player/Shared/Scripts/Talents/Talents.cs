using System.Collections.Generic;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Player.Shared.Scripts.Selection;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Talents
{
    // Each character's saved loadout and how many talent slots its mastery level gives
    public static class Talents
    {
        // One slot from the start, one more at each of these mastery levels
        private static readonly int[] ExtraSlotLevels = { 10, 20 };

        public static int SlotsFor(int masteryLevel)
        {
            int slots = 1;
            foreach (int level in ExtraSlotLevels)
                if (masteryLevel >= level) slots++;
            return slots;
        }

        // Mastery level of the next extra slot, or 0 when every slot is unlocked
        public static int NextSlotLevel(int masteryLevel)
        {
            foreach (int level in ExtraSlotLevels)
                if (masteryLevel < level) return level;
            return 0;
        }

        public static int Slots(CharacterDefinition character) => SlotsFor(Mastery.Level(character.name));

        public static bool IsUnlocked(CharacterDefinition character, TalentDefinition talent) =>
            Mastery.Level(character.name) >= talent.UnlockLevel;

        // The saved loadout, skipping talents that are gone or locked and anything past the slot count
        public static List<TalentDefinition> Equipped(CharacterDefinition character)
        {
            var equipped = new List<TalentDefinition>();
            string saved = PlayerPrefs.GetString(Key(character), string.Empty);
            if (saved.Length == 0) return equipped;

            int slots = Slots(character);
            foreach (string name in saved.Split(','))
            {
                if (equipped.Count >= slots) break;
                var talent = character.Talents.Find(t => t != null && t.name == name);
                if (talent != null && IsUnlocked(character, talent) && !equipped.Contains(talent))
                    equipped.Add(talent);
            }
            return equipped;
        }

        public static void SetEquipped(CharacterDefinition character, List<TalentDefinition> talents)
        {
            var names = new string[talents.Count];
            for (int i = 0; i < talents.Count; i++) names[i] = talents[i].name;
            PlayerPrefs.SetString(Key(character), string.Join(",", names));
            PlayerPrefs.Save();
        }

        // Attaches the equipped talents to a freshly spawned player
        public static void Apply(CharacterDefinition character, GameObject player)
        {
            foreach (var talent in Equipped(character))
            {
                if (talent.Prefab == null)
                {
                    Debug.LogWarning($"Talents: {talent.name} has no prefab.", talent);
                    continue;
                }
                Object.Instantiate(talent.Prefab, player.transform);
            }
        }

        private static string Key(CharacterDefinition character) => $"talents.{character.name}.equipped";
    }
}
