using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.MainMenu.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("In on-screen order, left to right")]
        [SerializeField] private Button[] buttons;

        private int _selectedIndex;

        private void Start()
        {
            if (buttons == null || buttons.Length == 0)
            {
                Debug.LogWarning("MainMenuController: No buttons assigned.", this);
                enabled = false;
                return;
            }

            _selectedIndex = 0;
            buttons[0].Select();
        }

        private void Update()
        {
            if (GameInput.NavigateRight || GameInput.NavigateDown) Move(1);
            else if (GameInput.NavigateLeft || GameInput.NavigateUp) Move(-1);

            if (GameInput.InteractPressed)
                buttons[_selectedIndex].onClick.Invoke();
        }

        private void Move(int step)
        {
            _selectedIndex = (_selectedIndex + step + buttons.Length) % buttons.Length;
            buttons[_selectedIndex].Select();
        }
    }
}
