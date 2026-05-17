using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.MainMenu.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        private Button[] _buttons;
        private int _selectedIndex;

        private void Start()
        {
            _buttons = new[] { startButton, quitButton };
            _selectedIndex = 0;
            _buttons[0].Select();
        }

        private void Update()
        {
            if (GameInput.NavigateLeft || GameInput.NavigateRight || GameInput.NavigateUp || GameInput.NavigateDown)
            {
                _selectedIndex = 1 - _selectedIndex;
                _buttons[_selectedIndex].Select();
            }

            if (GameInput.InteractPressed)
                _buttons[_selectedIndex].onClick.Invoke();
        }
    }
}
