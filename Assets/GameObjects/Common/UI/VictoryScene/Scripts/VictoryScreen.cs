using GameObjects.Common.Events;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.VictoryScene.Scripts
{
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button restartButton;

        private void Start()     => panel.SetActive(false);
        private void OnEnable()  => GlobalEvents.OnPlayerWon += Show;
        private void OnDisable() => GlobalEvents.OnPlayerWon -= Show;

        private void Update()
        {
            if (!panel.activeSelf) return;
            if (GameInput.InteractPressed)
                restartButton.onClick.Invoke();
        }

        private void Show()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
            restartButton.Select();
        }
    }
}
