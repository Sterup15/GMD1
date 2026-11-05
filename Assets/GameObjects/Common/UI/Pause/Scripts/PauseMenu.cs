using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Common.UI.Settings.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Pause.Scripts
{
    // Pause (Esc, P, gamepad Start) stops the run and offers Resume, Settings and Main Menu.
    // It doesn't open over another pausing screen (upgrade cards, victory, defeat), which already set the time scale to 0
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [Tooltip("Uses the MainMenuButton component, which restores the time scale before loading the menu")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SettingsMenu settingsPrefab;

        private MenuNavigator _navigator;
        private SettingsMenu _settings;

        private bool IsOpen => panel.activeSelf;
        private bool InSettings => _settings != null && _settings.IsOpen;

        private void Awake()
        {
            _navigator = new MenuNavigator(new[] { resumeButton, settingsButton, mainMenuButton });
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(OpenSettings);
            panel.SetActive(false);
        }

        private void Update()
        {
            if (InSettings || _navigator.InputBlocked) return;

            if (!IsOpen)
            {
                if (GameInput.PausePressed && Time.timeScale > 0f) Open();
                return;
            }

            if (GameInput.PausePressed)
            {
                Resume();
                return;
            }

            _navigator.Navigate();
            if (GameInput.InteractPressed) _navigator.Submit();
        }

        private void Open()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
            _navigator.Select(resumeButton);
        }

        private void Resume()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OpenSettings()
        {
            if (_settings == null) _settings = Instantiate(settingsPrefab);
            _settings.Open(OnSettingsClosed);
        }

        // Closing settings with Pause must not also resume the game on the same frame
        private void OnSettingsClosed()
        {
            _navigator.BlockInputThisFrame();
            _navigator.Select(settingsButton);
        }
    }
}
