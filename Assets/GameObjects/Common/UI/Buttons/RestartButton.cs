using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Scripts
{
    [RequireComponent(typeof(Button))]
    public class RestartButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(Restart);
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
