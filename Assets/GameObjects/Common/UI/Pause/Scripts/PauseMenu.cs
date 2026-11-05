using GameObjects.Common.UI.Settings.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private Button[] _buttons;
        private int _selectedIndex;
        private int _ignoreInputFrame = -1;
        private SettingsMenu _settings;

        private bool IsOpen => panel.activeSelf;
        private bool InSettings => _settings != null && _settings.IsOpen;

        private void Awake()
        {
            _buttons = new[] { resumeButton, settingsButton, mainMenuButton };
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(OpenSettings);
            panel.SetActive(false);
        }

        private void Update()
        {
            if (InSettings || Time.frameCount == _ignoreInputFrame) return;

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

            SyncWithMouseSelection();

            if (GameInput.NavigateDown || GameInput.NavigateRight)     Select(_selectedIndex + 1);
            else if (GameInput.NavigateUp || GameInput.NavigateLeft)   Select(_selectedIndex - 1);

            if (GameInput.InteractPressed)
                _buttons[_selectedIndex].onClick.Invoke();
        }

        private void Open()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
            Select(0);
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
            _ignoreInputFrame = Time.frameCount;
            Select(1);
        }

        private void SyncWithMouseSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            for (int i = 0; i < _buttons.Length; i++)
                if (_buttons[i].gameObject == selected) _selectedIndex = i;
        }

        private void Select(int index)
        {
            _selectedIndex = (index + _buttons.Length) % _buttons.Length;
            _buttons[_selectedIndex].Select();
        }
    }
}
