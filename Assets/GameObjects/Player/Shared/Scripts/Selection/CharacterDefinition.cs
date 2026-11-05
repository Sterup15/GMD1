using System.Collections.Generic;
using GameObjects.Player.Shared.Scripts.Talents;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // Everything the character select screen shows about a character, plus the prefab to spawn.
    // Base stats aren't repeated here: the screen reads them from the prefab's Stats component
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "BulletHell/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private Sprite portrait;
        [SerializeField] private GameObject prefab;
        [Tooltip("This character's talents, in the order the loadout screen lists them")]
        [SerializeField] private List<TalentDefinition> talents = new();

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Portrait    => portrait;
        public GameObject Prefab  => prefab;
        public List<TalentDefinition> Talents => talents;
    }
}
