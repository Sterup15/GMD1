using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Talents
{
    // One character talent: unlocked by that character's mastery level and chosen in the loadout before a run.
    // What it does lives in its prefab, which is attached to the player when the run starts, so a talent is a new
    // asset plus a prefab with its behaviour script, listed on the character's CharacterDefinition
    [CreateAssetMenu(fileName = "Talent", menuName = "BulletHell/Talent")]
    public class TalentDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;
        [Tooltip("Mastery level of its character that unlocks it")]
        [SerializeField, Min(1)] private int unlockLevel = 1;
        [Tooltip("Spawned as a child of the player when the talent is equipped. Its scripts find the player's ActorEvents and Stats in their parent")]
        [SerializeField] private GameObject prefab;

        public string DisplayName => displayName;
        public string Description => description;
        public int UnlockLevel    => unlockLevel;
        public GameObject Prefab  => prefab;
    }
}
