using System;
using GameObjects.Common.UI.Navigation.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Settings.Scripts
{
    // The settings page. Opened from the main menu and the pause menu, which pass a callback to return to.
    // Up/down picks a row, left/right changes it, Interact presses it, and Pause or the Back button closes the page
    public class SettingsMenu : MenuScreen
    {
        [SerializeField] private SettingsRow[] rows;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            // One navigator row per setting, so up/down moves between them and left/right is free to change the value
            var items = new Button[rows.Length + 1][];
            for (int i = 0; i < rows.Length; i++)
                items[i] = new[] { rows[i].GetComponent<Button>() };
            items[rows.Length] = new[] { backButton };
            Navigator = new MenuNavigator(items);

            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            ShowPanel(onClosed);
            Navigator.Select(0, 0);
        }

        protected override void HandleExtraInput()
        {
            if (Navigator.Row >= rows.Length) return;
            if (GameInput.NavigateLeft)       rows[Navigator.Row].Decrease();
            else if (GameInput.NavigateRight) rows[Navigator.Row].Increase();
        }
    }
}
