using System;
using GameObjects.Common.Armory.Scripts;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Common.UI.Loadout.Scripts;
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
    // rows). Armory opens the shop for permanent upgrades on top of this screen, and Start opens the chosen
    // character's talent loadout, from which the run begins. Pause also goes back
    public class CharacterSelectScreen : MenuScreen
    {
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
        [Tooltip("Talent loadout shown after Start. Without it (or for a character with no talents) Start begins the run at once")]
        [SerializeField] private LoadoutScreen loadoutPrefab;
        [SerializeField] private int gameSceneIndex = 1;

        private const int PortraitRowIndex = 0;

        private int _characterIndex;
        private ArmoryScreen _armory;
        private LoadoutScreen _loadout;

        // While the Armory or the loadout is open on top, it has the input
        protected override bool IsCovered => (_armory != null && _armory.IsOpen) || (_loadout != null && _loadout.IsOpen);

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
                portrait.onClick.AddListener(() => Navigator.Select(PortraitRowIndex, index));
                portraits[i] = portrait;
            }

            Navigator = new MenuNavigator(portraits, new[] { backButton, armoryButton, startButton });
            Navigator.OnSelectionChanged += OnSelectionChanged;
            startButton.onClick.AddListener(OpenLoadout);
            backButton.onClick.AddListener(Close);
            if (armoryPrefab != null) armoryButton.onClick.AddListener(OpenArmory);
            else armoryButton.interactable = false;
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            ShowPanel(onClosed);
            // Selecting Start first makes it the button that moving down from the portraits lands on
            Navigator.Select(startButton);
            Navigator.Select(PortraitRowIndex, CharacterSelection.LastPickIndex(roster));
        }

        // Interact on a portrait moves down to Start; elsewhere it presses the selected button
        protected override void Interact()
        {
            if (Navigator.Row == PortraitRowIndex) Navigator.Select(startButton);
            else Navigator.Submit();
        }

        private void OpenArmory()
        {
            if (_armory == null) _armory = Instantiate(armoryPrefab);
            _armory.Open(OnArmoryClosed);
        }

        // The key that closed the Armory must not also act on this screen
        private void OnArmoryClosed()
        {
            Navigator.BlockInputThisFrame();
            Navigator.Select(armoryButton);
        }

        private void OpenLoadout()
        {
            var character = roster[_characterIndex];
            if (loadoutPrefab == null || character.Talents.Count == 0)
            {
                StartRun();
                return;
            }

            if (_loadout == null) _loadout = Instantiate(loadoutPrefab);
            _loadout.Open(character, StartRun, OnLoadoutClosed);
        }

        private void OnLoadoutClosed()
        {
            Navigator.BlockInputThisFrame();
            Navigator.Select(startButton);
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
