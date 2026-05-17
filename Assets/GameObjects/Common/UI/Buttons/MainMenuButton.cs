using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Scripts
{
    [RequireComponent(typeof(Button))]
    public class MainMenuButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(GoToMainMenu);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }
}
