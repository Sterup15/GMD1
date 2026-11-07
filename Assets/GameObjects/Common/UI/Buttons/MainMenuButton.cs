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

        // GamePause drops every pause when the scene loads
        private void GoToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}
