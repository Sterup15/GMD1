using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // Spawns the character picked on the select screen. Runs in Awake, so everything that looks for the
    // "Player" tag in Start (camera, spawner, HUD, upgrades) finds it
    [DefaultExecutionOrder(-50)]
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private CharacterRoster roster;

        private void Awake()
        {
            if (roster == null || roster.Count == 0)
            {
                Debug.LogError("PlayerSpawner: No character roster assigned.", this);
                return;
            }

            // Playing the game scene directly (no pick yet) uses the last picked character, and selects it so the
            // rest of the run (e.g. mastery XP) knows who is playing
            var character = CharacterSelection.Selected;
            if (character == null || roster.IndexOf(character) < 0)
            {
                character = roster[CharacterSelection.LastPickIndex(roster)];
                CharacterSelection.Select(character);
            }

            Instantiate(character.Prefab, transform.position, Quaternion.identity);
        }
    }
}
