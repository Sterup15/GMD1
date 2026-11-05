using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // The character picked on the select screen. Static so it survives the scene load and restarts,
    // and the last pick is saved so the select screen starts on it next session
    public static class CharacterSelection
    {
        private const string LastPickKey = "character.lastPick";

        public static CharacterDefinition Selected { get; private set; }

        public static void Select(CharacterDefinition character)
        {
            Selected = character;
            PlayerPrefs.SetString(LastPickKey, character.name);
            PlayerPrefs.Save();
        }

        // Index of the last picked character in the roster, or 0 if there is none (first session, or it was removed)
        public static int LastPickIndex(CharacterRoster roster)
        {
            string last = PlayerPrefs.GetString(LastPickKey, string.Empty);
            for (int i = 0; i < roster.Count; i++)
                if (roster[i].name == last) return i;
            return 0;
        }
    }
}
