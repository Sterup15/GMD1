using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Scripts
{
    [RequireComponent(typeof(Button))]
    public class StartGameButton : MonoBehaviour
    {
        [SerializeField] private int gameSceneIndex = 1;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(StartGame);
        }

        private void StartGame() => SceneManager.LoadScene(gameSceneIndex);
    }
}
