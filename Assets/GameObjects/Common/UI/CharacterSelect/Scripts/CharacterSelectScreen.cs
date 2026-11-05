using System;
using GameObjects.Player.Shared.Scripts.Selection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.CharacterSelect.Scripts
{
    // Pick a character before a run. One portrait per roster entry, built when the screen is created, so a new
    // character only needs a CharacterDefinition in the roster. Two rows: the portraits (left/right or a click picks
    // a character, Interact moves down to Start) and Back / Talents / Start below them (down/up moves between the
    // rows, disabled buttons are skipped). Pause also goes back
    public class CharacterSelectScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CharacterRoster roster;
        [Tooltip("Inactive template, copied once per character into the portrait row")]
        [SerializeField] private Button portraitTemplate;
        [SerializeField] private Transform portraitRow;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [Tooltip("Opens the chosen character's talents. Not interactable until the talent system exists")]
        [SerializeField] private Button talentsButton;
        [SerializeField] private int gameSceneIndex = 1;

        private Button[] _portraits;
        private Button[] _bottomButtons;
        private int _selectedIndex;
        private bool _onBottomRow;
        private int _bottomIndex;
        private int _openedFrame = -1;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            _portraits = new Button[roster.Count];
            for (int i = 0; i < roster.Count; i++)
            {
                var portrait = Instantiate(portraitTemplate, portraitRow);
                portrait.gameObject.SetActive(true);
                portrait.name = roster[i].name;
                portrait.GetComponent<Image>().sprite = roster[i].Portrait;
                int index = i;
                portrait.onClick.AddListener(() => Show(index));
                _portraits[i] = portrait;
            }

            _bottomButtons = new[] { backButton, talentsButton, startButton };
            startButton.onClick.AddListener(StartRun);
            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            _openedFrame = Time.frameCount;
            panel.SetActive(true);
            _onBottomRow = false;
            Show(CharacterSelection.LastPickIndex(roster));
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            // The key press that opened the screen must not also act on it
            if (!IsOpen || Time.frameCount == _openedFrame) return;

            if (GameInput.PausePressed)
            {
                Close();
                return;
            }

            SyncWithMouseSelection();

            if (GameInput.NavigateDown && !_onBottomRow)
            {
                _onBottomRow = true;
                SelectBottom(Array.IndexOf(_bottomButtons, startButton), 1);
            }
            else if (GameInput.NavigateUp && _onBottomRow)
            {
                _onBottomRow = false;
                Show(_selectedIndex);
            }
            else if (GameInput.NavigateRight || GameInput.NavigateLeft)
            {
                int step = GameInput.NavigateRight ? 1 : -1;
                if (_onBottomRow) SelectBottom(_bottomIndex + step, step);
                else Show(_selectedIndex + step);
            }

            if (GameInput.InteractPressed)
            {
                if (_onBottomRow) _bottomButtons[_bottomIndex].onClick.Invoke();
                else
                {
                    _onBottomRow = true;
                    SelectBottom(Array.IndexOf(_bottomButtons, startButton), 1);
                }
            }
        }

        // A mouse click on Back or Start moves the keyboard focus to that row too
        private void SyncWithMouseSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            for (int i = 0; i < _bottomButtons.Length; i++)
            {
                if (_bottomButtons[i].gameObject != selected) continue;
                _onBottomRow = true;
                _bottomIndex = i;
                return;
            }
            foreach (var portrait in _portraits)
                if (portrait.gameObject == selected) _onBottomRow = false;
        }

        // Steps past buttons that aren't interactable yet (e.g. Talents), in the direction of travel
        private void SelectBottom(int index, int step)
        {
            for (int tries = 0; tries < _bottomButtons.Length; tries++)
            {
                _bottomIndex = (index + _bottomButtons.Length) % _bottomButtons.Length;
                if (_bottomButtons[_bottomIndex].interactable) break;
                index += step;
            }
            _bottomButtons[_bottomIndex].Select();
        }

        private void Show(int index)
        {
            _selectedIndex = (index + _portraits.Length) % _portraits.Length;
            _portraits[_selectedIndex].Select();

            var character = roster[_selectedIndex];
            nameText.text = character.DisplayName;
            descriptionText.text = character.Description;
            statsText.text = StatsLine(character);
        }

        private static string StatsLine(CharacterDefinition character)
        {
            var stats = character.Prefab.GetComponent<Stats.Scripts.Stats>();
            if (stats == null) return string.Empty;
            return $"Health {stats.MaxHealth.BaseValue:0}    Damage {stats.Damage.BaseValue:0}    " +
                   $"Attack speed {stats.FireRate.BaseValue:0.##}    Move speed {stats.MoveSpeed.BaseValue:0.#}    " +
                   $"Range {stats.ShootRange.BaseValue:0.#}";
        }

        private void StartRun()
        {
            CharacterSelection.Select(roster[_selectedIndex]);
            SceneManager.LoadScene(gameSceneIndex);
        }
    }
}
