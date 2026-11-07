using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Common.UI.Settings.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Pause.Scripts
{
    // Pause (Esc, P, gamepad Start) stops the run and offers Resume, Settings and Main Menu; Pause again resumes.
    // It doesn't open over another pausing screen (upgrade cards, victory, defeat), which already holds a pause
    public class PauseMenu : MenuScreen
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SettingsMenu settingsPrefab;

        private SettingsMenu _settings;

        protected override bool IsCovered => _settings != null && _settings.IsOpen;

        private void Awake()
        {
            Navigator = new MenuNavigator(new[] { resumeButton, settingsButton, mainMenuButton });
            resumeButton.onClick.AddListener(Close);
            settingsButton.onClick.AddListener(OpenSettings);
            panel.SetActive(false);
        }

        protected override void Update()
        {
            if (!IsOpen)
            {
                if (GameInput.PausePressed && !GamePause.IsPaused && !Navigator.InputBlocked) Open();
                return;
            }
            base.Update();
        }

        private void Open()
        {
            ShowPanel();
            GamePause.Pause(this);
            Navigator.Select(resumeButton);
        }

        protected override void OnClosed() => GamePause.Resume(this);

        private void OpenSettings()
        {
            if (_settings == null) _settings = Instantiate(settingsPrefab);
            _settings.Open(OnSettingsClosed);
        }

        // Closing settings with Pause must not also resume the game on the same frame
        private void OnSettingsClosed()
        {
            Navigator.BlockInputThisFrame();
            Navigator.Select(settingsButton);
        }
    }
}
