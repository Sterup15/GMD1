using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Talents
{
    // One character talent: unlocked by that character's mastery level and chosen in the loadout before a run.
    // What it does lives in its prefab, which is attached to the player when the run starts, so a talent is a new
    // asset plus a prefab with its behaviour script, listed on the character's CharacterDefinition.
    // Level 1 comes with the unlock; each entry in levels is one more level, bought with Glory once mastery allows
    [CreateAssetMenu(fileName = "Talent", menuName = "BulletHell/Talent")]
    public class TalentDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;
        [Tooltip("Mastery level of its character that unlocks it")]
        [SerializeField, Min(1)] private int unlockLevel = 1;
        [Tooltip("Spawned as a child of the player when the talent is equipped. Its scripts find the player's ActorEvents and Stats in their parent")]
        [SerializeField] private GameObject prefab;
        [Tooltip("Levels 2, 3, ... in order. The prefab reads its level through ILeveledTalent")]
        [SerializeField] private List<TalentLevel> levels = new();

        public string DisplayName => displayName;
        public string Description => description;
        public int UnlockLevel    => unlockLevel;
        public GameObject Prefab  => prefab;
        public int MaxLevel       => 1 + levels.Count;

        // Level 2 and up, or null for level 1 and levels past the max
        public TalentLevel LevelInfo(int level) => level >= 2 && level - 2 < levels.Count ? levels[level - 2] : null;

        public string DescriptionAt(int level) => LevelInfo(level)?.Description ?? description;
    }
}
