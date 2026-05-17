using GameObjects.Common.Events;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.VictoryScene.Scripts
{
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private Button[] _buttons;
        private int _selectedIndex;

        private void Start()
        {
            _buttons = new[] { restartButton, mainMenuButton };
            panel.SetActive(false);
        }

        private void OnEnable()  => GlobalEvents.OnPlayerWon += Show;
        private void OnDisable() => GlobalEvents.OnPlayerWon -= Show;

        private void Update()
        {
            if (!panel.activeSelf) return;

            if (GameInput.NavigateLeft || GameInput.NavigateRight || GameInput.NavigateUp || GameInput.NavigateDown)
            {
                _selectedIndex = 1 - _selectedIndex;
                _buttons[_selectedIndex].Select();
            }

            if (GameInput.InteractPressed)
                _buttons[_selectedIndex].onClick.Invoke();
        }

        private void Show()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
            _selectedIndex = 0;
            _buttons[0].Select();
        }
    }
}
