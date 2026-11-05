using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private Button[] _items;
        private int _selectedIndex;
        private int _openedFrame = -1;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            _items = new Button[rows.Length + 1];
            for (int i = 0; i < rows.Length; i++)
                _items[i] = rows[i].GetComponent<Button>();
            _items[rows.Length] = backButton;

            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            _openedFrame = Time.frameCount;
            panel.SetActive(true);
            Select(0);
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            // The key press that opened the page must not also act on it
            if (!IsOpen || Time.frameCount == _openedFrame) return;

            if (GameInput.PausePressed)
            {
                Close();
                return;
            }

            SyncWithMouseSelection();

            if (GameInput.NavigateDown)      Select(_selectedIndex + 1);
            else if (GameInput.NavigateUp)   Select(_selectedIndex - 1);

            if (_selectedIndex < rows.Length)
            {
                if (GameInput.NavigateLeft)       rows[_selectedIndex].Decrease();
                else if (GameInput.NavigateRight) rows[_selectedIndex].Increase();
            }

            if (GameInput.InteractPressed)
                _items[_selectedIndex].onClick.Invoke();
        }

        // A click selects a row through the EventSystem, so keyboard navigation continues from there
        private void SyncWithMouseSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            for (int i = 0; i < _items.Length; i++)
                if (_items[i].gameObject == selected) _selectedIndex = i;
        }

        private void Select(int index)
        {
            _selectedIndex = (index + _items.Length) % _items.Length;
            _items[_selectedIndex].Select();
        }
    }
}
