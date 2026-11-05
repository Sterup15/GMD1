using GameObjects.Common.UI.Settings.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameObjects.Common.UI.MainMenu.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("In on-screen order, left to right")]
        [SerializeField] private Button[] buttons;
        [Tooltip("Optional: the button that opens the settings page (also listed in buttons)")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPrefab;

        private int _selectedIndex;
        private int _ignoreInputFrame = -1;
        private SettingsMenu _settings;

        private bool InSettings => _settings != null && _settings.IsOpen;

        private void Start()
        {
            if (buttons == null || buttons.Length == 0)
            {
                Debug.LogWarning("MainMenuController: No buttons assigned.", this);
                enabled = false;
                return;
            }

            if (settingsButton != null && settingsPrefab != null)
                settingsButton.onClick.AddListener(OpenSettings);

            Select(0);
        }

        private void Update()
        {
            if (InSettings || Time.frameCount == _ignoreInputFrame) return;

            SyncWithMouseSelection();

            if (GameInput.NavigateRight || GameInput.NavigateDown) Select(_selectedIndex + 1);
            else if (GameInput.NavigateLeft || GameInput.NavigateUp) Select(_selectedIndex - 1);

            if (GameInput.InteractPressed)
                buttons[_selectedIndex].onClick.Invoke();
        }

        private void OpenSettings()
        {
            if (_settings == null) _settings = Instantiate(settingsPrefab);
            _settings.Open(OnSettingsClosed);
        }

        // The key that closed settings must not also press a main menu button on the same frame
        private void OnSettingsClosed()
        {
            _ignoreInputFrame = Time.frameCount;
            settingsButton.Select();
        }

        private void SyncWithMouseSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i].gameObject == selected) _selectedIndex = i;
        }

        private void Select(int index)
        {
            _selectedIndex = (index + buttons.Length) % buttons.Length;
            buttons[_selectedIndex].Select();
        }
    }
}
