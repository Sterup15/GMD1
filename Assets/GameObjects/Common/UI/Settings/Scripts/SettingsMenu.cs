using System;
using GameObjects.Common.UI.Navigation.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Settings.Scripts
{
    // The settings page. Opened from the main menu and the pause menu, which pass a callback to return to.
    // Up/down picks a row, left/right changes it, Interact presses it, and Pause or the Back button closes the page
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private SettingsRow[] rows;
        [SerializeField] private Button backButton;

        private MenuNavigator _navigator;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            // One navigator row per setting, so up/down moves between them and left/right is free to change the value
            var items = new Button[rows.Length + 1][];
            for (int i = 0; i < rows.Length; i++)
                items[i] = new[] { rows[i].GetComponent<Button>() };
            items[rows.Length] = new[] { backButton };
            _navigator = new MenuNavigator(items);

            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            panel.SetActive(true);
            _navigator.BlockInputThisFrame();
            _navigator.Select(0, 0);
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen || _navigator.InputBlocked) return;

            if (GameInput.PausePressed)
            {
                Close();
                return;
            }

            _navigator.Navigate();

            if (_navigator.Row < rows.Length)
            {
                if (GameInput.NavigateLeft)       rows[_navigator.Row].Decrease();
                else if (GameInput.NavigateRight) rows[_navigator.Row].Increase();
            }

            if (GameInput.InteractPressed) _navigator.Submit();
        }
    }
}
