using GameObjects.Common.UI.CharacterSelect.Scripts;
using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Common.UI.Settings.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.MainMenu.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("In on-screen order, left to right")]
        [SerializeField] private Button[] buttons;
        [Tooltip("Opens the character select screen (also listed in buttons)")]
        [SerializeField] private Button playButton;
        [SerializeField] private CharacterSelectScreen characterSelectPrefab;
        [Tooltip("Opens the settings page (also listed in buttons)")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPrefab;

        private MenuNavigator _navigator;
        private SettingsMenu _settings;
        private CharacterSelectScreen _characterSelect;

        // While a screen opened from the menu is showing, it handles the input
        private bool ScreenOpen => (_settings != null && _settings.IsOpen) || (_characterSelect != null && _characterSelect.IsOpen);

        private void Start()
        {
            if (buttons == null || buttons.Length == 0)
            {
                Debug.LogWarning("MainMenuController: No buttons assigned.", this);
                enabled = false;
                return;
            }

            if (playButton != null && characterSelectPrefab != null)
                playButton.onClick.AddListener(OpenCharacterSelect);
            if (settingsButton != null && settingsPrefab != null)
                settingsButton.onClick.AddListener(OpenSettings);

            _navigator = new MenuNavigator(buttons);
            _navigator.Select(0, 0);
        }

        private void Update()
        {
            if (ScreenOpen || _navigator.InputBlocked) return;

            _navigator.Navigate();
            if (GameInput.InteractPressed) _navigator.Submit();
        }

        private void OpenCharacterSelect()
        {
            if (_characterSelect == null) _characterSelect = Instantiate(characterSelectPrefab);
            _characterSelect.Open(() => ReturnTo(playButton));
        }

        private void OpenSettings()
        {
            if (_settings == null) _settings = Instantiate(settingsPrefab);
            _settings.Open(() => ReturnTo(settingsButton));
        }

        // The key that closed a screen must not also press a main menu button on the same frame
        private void ReturnTo(Button button)
        {
            _navigator.BlockInputThisFrame();
            _navigator.Select(button);
        }
    }
}
