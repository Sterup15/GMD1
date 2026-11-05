using System;
using GameObjects.Common.Armory.Scripts;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Player.Shared.Scripts.Selection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameObjects.Common.UI.CharacterSelect.Scripts
{
    // Pick a character before a run. One portrait per roster entry, built when the screen is created, so a new
    // character only needs a CharacterDefinition in the roster. Two rows: the portraits (left/right or a click picks
    // a character, Interact moves down to Start) and Back / Armory / Start below them (down/up moves between the
    // rows). Armory opens the shop for permanent upgrades on top of this screen. Pause also goes back
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
        [SerializeField] private Button armoryButton;
        [SerializeField] private ArmoryScreen armoryPrefab;
        [SerializeField] private int gameSceneIndex = 1;

        private const int PortraitRowIndex = 0;

        private MenuNavigator _navigator;
        private int _characterIndex;
        private Action _onClosed;
        private ArmoryScreen _armory;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            var portraits = new Button[roster.Count];
            for (int i = 0; i < roster.Count; i++)
            {
                var portrait = Instantiate(portraitTemplate, portraitRow);
                portrait.gameObject.SetActive(true);
                portrait.name = roster[i].name;
                portrait.GetComponent<Image>().sprite = roster[i].Portrait;
                int index = i;
                portrait.onClick.AddListener(() => _navigator.Select(PortraitRowIndex, index));
                portraits[i] = portrait;
            }

            _navigator = new MenuNavigator(portraits, new[] { backButton, armoryButton, startButton });
            _navigator.OnSelectionChanged += OnSelectionChanged;
            startButton.onClick.AddListener(StartRun);
            backButton.onClick.AddListener(Close);
            if (armoryPrefab != null) armoryButton.onClick.AddListener(OpenArmory);
            else armoryButton.interactable = false;
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            panel.SetActive(true);
            _navigator.BlockInputThisFrame();
            // Selecting Start first makes it the button that moving down from the portraits lands on
            _navigator.Select(startButton);
            _navigator.Select(PortraitRowIndex, CharacterSelection.LastPickIndex(roster));
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            // While the Armory is open on top, it has the input
            if (!IsOpen || _navigator.InputBlocked || (_armory != null && _armory.IsOpen)) return;

            if (GameInput.PausePressed)
            {
                Close();
                return;
            }

            _navigator.Navigate();

            if (!GameInput.InteractPressed) return;
            if (_navigator.Row == PortraitRowIndex) _navigator.Select(startButton);
            else _navigator.Submit();
        }

        private void OpenArmory()
        {
            if (_armory == null) _armory = Instantiate(armoryPrefab);
            _armory.Open(OnArmoryClosed);
        }

        // The key that closed the Armory must not also act on this screen
        private void OnArmoryClosed()
        {
            _navigator.BlockInputThisFrame();
            _navigator.Select(armoryButton);
        }

        private void OnSelectionChanged(int row, int column)
        {
            if (row == PortraitRowIndex) Show(column);
        }

        private void Show(int index)
        {
            _characterIndex = index;
            var character = roster[index];
            nameText.text = $"{character.DisplayName}  <size=60%>{MasteryLine(character)}</size>";
            descriptionText.text = character.Description;
            statsText.text = StatsLine(character);
        }

        private static string MasteryLine(CharacterDefinition character)
        {
            int xp = Mastery.Xp(character.name);
            int level = Mastery.LevelFor(xp);
            if (level >= Mastery.MaxLevel) return $"Mastery {level} (max)";

            int progress = xp - Mastery.XpToReach(level);
            int needed = Mastery.XpToReach(level + 1) - Mastery.XpToReach(level);
            return $"Mastery {level}  ({progress}/{needed} XP)";
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
            CharacterSelection.Select(roster[_characterIndex]);
            SceneManager.LoadScene(gameSceneIndex);
        }
    }
}
