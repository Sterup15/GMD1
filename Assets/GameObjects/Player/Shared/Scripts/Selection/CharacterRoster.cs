using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // The playable characters in select-screen order. Shared by the select screen and PlayerSpawner,
    // so adding a character is a new CharacterDefinition added here, with no UI or code changes
    [CreateAssetMenu(fileName = "CharacterRoster", menuName = "BulletHell/Character Roster")]
    public class CharacterRoster : ScriptableObject
    {
        [SerializeField] private CharacterDefinition[] characters;

        public int Count => characters.Length;
        public CharacterDefinition this[int index] => characters[index];

        public int IndexOf(CharacterDefinition character) => System.Array.IndexOf(characters, character);
    }
}
