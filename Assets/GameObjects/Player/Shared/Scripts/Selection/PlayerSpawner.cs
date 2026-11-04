using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Selection
{
    // Spawns the character picked in the main menu. Runs in Awake, so everything that looks for the
    // "Player" tag in Start (camera, spawner, HUD, upgrades) finds it
    [DefaultExecutionOrder(-50)]
    public class PlayerSpawner : MonoBehaviour
    {
        [Tooltip("Indexed by CharacterSelection.SelectedIndex, in the same order as the main menu buttons")]
        [SerializeField] private GameObject[] characters;

        private void Awake()
        {
            if (characters == null || characters.Length == 0)
            {
                Debug.LogError("PlayerSpawner: No character prefabs assigned.", this);
                return;
            }

            int index = Mathf.Clamp(CharacterSelection.SelectedIndex, 0, characters.Length - 1);
            Instantiate(characters[index], transform.position, Quaternion.identity);
        }
    }
}
