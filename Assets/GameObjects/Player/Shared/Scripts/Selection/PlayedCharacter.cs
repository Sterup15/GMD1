using GameObjects.Common.Progression.Scripts;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // Which CharacterDefinition this player was spawned from. Added by PlayerSpawner, and read by run rewards
    // (RunPoints) through IMasteryCharacter so they don't need to know about character selection
    public class PlayedCharacter : MonoBehaviour, IMasteryCharacter
    {
        public CharacterDefinition Definition { get; set; }

        public string MasteryId   => Definition != null ? Definition.name : string.Empty;
        public string DisplayName => Definition != null ? Definition.DisplayName : string.Empty;
    }
}
