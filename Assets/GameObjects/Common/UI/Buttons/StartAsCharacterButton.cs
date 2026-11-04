using GameObjects.Player.Shared.Scripts.Selection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Scripts
{
    // Starts a run as the given character (index into PlayerSpawner's character list)
    [RequireComponent(typeof(Button))]
    public class StartAsCharacterButton : MonoBehaviour
    {
        [SerializeField] private int characterIndex;
        [SerializeField] private int gameSceneIndex = 1;

        private void Start() => GetComponent<Button>().onClick.AddListener(StartGame);

        private void StartGame()
        {
            CharacterSelection.SelectedIndex = characterIndex;
            SceneManager.LoadScene(gameSceneIndex);
        }
    }
}
