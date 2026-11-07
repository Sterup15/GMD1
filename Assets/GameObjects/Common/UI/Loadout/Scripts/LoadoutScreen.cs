using System;
using System.Collections.Generic;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Player.Shared.Scripts.Selection;
using GameObjects.Player.Shared.Scripts.Talents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Loadout.Scripts
{
    // The talent loadout, shown after Start on the character select screen. One row per talent of the chosen
    // character; the details box beside the list shows the selected talent at one of its levels. Left/right (or the
    // box's arrows) browses the bought levels and the next one, and Interact or the box's button does what the
    // button says for that level: buy it, equip it, or unequip it; clicking a row only shows it. Equipping with every slot full swaps out the
    // oldest pick. Begin starts the run, Back or Pause returns to the select screen. The loadout is saved per character
    public class LoadoutScreen : MenuScreen
    {
        [Tooltip("Inactive template, copied once per talent")]
        [SerializeField] private LoadoutRow rowTemplate;
        [Tooltip("Distance between rows, going down from the template's position")]
        [SerializeField] private float rowSpacing = 85f;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI slotsText;
        [Tooltip("Hint line under the list")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button beginButton;

        [Header("Details box")]
        [SerializeField] private GameObject detailsBox;
        [SerializeField] private TextMeshProUGUI detailsTitle;
        [SerializeField] private TextMeshProUGUI detailsLevel;
        [SerializeField] private TextMeshProUGUI detailsBody;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [Tooltip("Buys, equips or unequips the viewed level, and says which it will do")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionLabel;
        [Tooltip("Price colour when the player has too little Glory")]
        [SerializeField] private Color unaffordableColor = new(0.7f, 0.23f, 0.23f);

        private readonly List<LoadoutRow> _rows = new();
        private List<TalentDefinition> _equipped;
        private CharacterDefinition _character;
        private LoadoutRow _shownRow;
        private Color _actionColor;
        private Action _onBegin;

        private void Awake()
        {
            backButton.onClick.AddListener(Close);
            beginButton.onClick.AddListener(Begin);
            decreaseButton.onClick.AddListener(ShowPreviousLevel);
            increaseButton.onClick.AddListener(ShowNextLevel);
            actionButton.onClick.AddListener(Act);
            _actionColor = actionLabel.color;
            panel.SetActive(false);
        }

        public void Open(CharacterDefinition character, Action onBegin, Action onClosed)
        {
            _character = character;
            _onBegin = onBegin;
            _equipped = Talents.Equipped(character);

            // Rows cache their button in Awake, so the panel must be active before they are built
            ShowPanel(onClosed);
            BuildRows();
            Navigator.BlockInputThisFrame();
            _shownRow = _rows.Count > 0 ? _rows[0] : null;
            detailsBox.SetActive(_shownRow != null);
            Refresh();
            // Begin first, so moving down past the last talent lands on Begin, and Begin is selected when there
            // are no talents yet
            Navigator.Select(beginButton);
            if (_rows.Count > 0) Navigator.Select(0, 0);
        }

        protected override void HandleExtraInput()
        {
            if (Navigator.Row >= _rows.Count) return;
            if (GameInput.NavigateLeft)       ShowPreviousLevel();
            else if (GameInput.NavigateRight) ShowNextLevel();
        }

        // On a talent, Interact does what the details box's button says; elsewhere it presses the selected button
        protected override void Interact()
        {
            if (Navigator.Row < _rows.Count) Act();
            else                             Navigator.Submit();
        }

        // Rows depend on the character, so they are rebuilt on every open (a menu, not a per-frame path)
        private void BuildRows()
        {
            foreach (var row in _rows) Destroy(row.gameObject);
            _rows.Clear();

            var talents = _character.Talents;
            var items = new List<Button[]>();

            for (int i = 0; i < talents.Count; i++)
            {
                if (talents[i] == null) continue;
                var row = CreateRow(rowTemplate, _rows.Count, rowSpacing);
                row.name = talents[i].name;
                row.Bind(talents[i], Talents.EquippedLevel(talents[i]));
                // A click only shows the talent; buying and equipping go through the details box's button
                row.Button.onClick.AddListener(() => ShowDetails(row));
                _rows.Add(row);
                // One navigator row per talent, so left/right is free to browse levels
                items.Add(new[] { row.Button });
            }
            items.Add(new[] { backButton, beginButton });

            Navigator = new MenuNavigator(items.ToArray());
            Navigator.OnSelectionChanged += ShowDescription;
        }

        private void ShowPreviousLevel() => ViewLevel(-1);
        private void ShowNextLevel()     => ViewLevel(1);

        private void ViewLevel(int step)
        {
            if (_shownRow != null && _shownRow.View(_shownRow.ViewedLevel + step)) ShowDetails(_shownRow);
        }

        // What the action button says, for the shown talent at its viewed level
        private void Act()
        {
            if (_shownRow == null) return;
            var talent = _shownRow.Talent;
            int viewed = _shownRow.ViewedLevel;

            if (viewed > Talents.Level(talent)) Upgrade(talent);
            else if (IsEquippedAt(talent, viewed)) Unequip(talent);
            else Equip(talent, viewed);
        }

        private void Upgrade(TalentDefinition talent)
        {
            if (!Talents.TryUpgrade(_character, talent)) return;
            Refresh();
        }

        private void Equip(TalentDefinition talent, int level)
        {
            if (!Talents.IsUnlocked(_character, talent)) return;

            if (!_equipped.Contains(talent))
            {
                if (_equipped.Count >= Talents.Slots(_character)) _equipped.RemoveAt(0);
                _equipped.Add(talent);
                Talents.SetEquipped(_character, _equipped);
            }
            Talents.SetEquippedLevel(talent, level);
            Refresh();
        }

        private void Unequip(TalentDefinition talent)
        {
            _equipped.Remove(talent);
            Talents.SetEquipped(_character, _equipped);
            Refresh();
        }

        private bool IsEquippedAt(TalentDefinition talent, int level) =>
            _equipped.Contains(talent) && Talents.EquippedLevel(talent) == level;

        private void Refresh()
        {
            int mastery = Mastery.Level(_character.name);
            int slots = Talents.SlotsFor(mastery);
            int nextSlot = Talents.NextSlotLevel(mastery);

            titleText.text = $"{_character.DisplayName} Talents";
            slotsText.text = $"Mastery {mastery}    Slots {_equipped.Count}/{slots}" +
                             (nextSlot > 0 ? $"  <size=75%>(next at Mastery {nextSlot})</size>" : string.Empty) +
                             $"    Glory {Glory.Balance}";

            foreach (var row in _rows)
            {
                bool equipped = _equipped.Contains(row.Talent);
                row.Refresh(Talents.IsUnlocked(_character, row.Talent), equipped, Talents.Level(row.Talent),
                            equipped ? Talents.EquippedLevel(row.Talent) : Talents.Level(row.Talent));
            }

            if (_shownRow != null) ShowDetails(_shownRow);
            ShowDescription(Navigator.Row, Navigator.Column);
        }

        private void ShowDescription(int row, int column)
        {
            if (row < _rows.Count)
            {
                ShowDetails(_rows[row]);
                descriptionText.text = string.Empty;
            }
            else if (_rows.Count == 0) descriptionText.text = "No talents for this character yet";
            else                       descriptionText.text = column == 1 ? "Start the run with this loadout" : string.Empty;
        }

        // Name, "Level 2 / 3", what the viewed level does, the arrows and the button for that level
        private void ShowDetails(LoadoutRow row)
        {
            _shownRow = row;
            var talent = row.Talent;
            int viewed = row.ViewedLevel;

            detailsTitle.text = talent.DisplayName;
            detailsLevel.text = $"Level {viewed} / {talent.MaxLevel}";
            detailsBody.text = talent.DescriptionAt(viewed);

            bool hasLevels = talent.MaxLevel > 1;
            decreaseButton.gameObject.SetActive(hasLevels);
            increaseButton.gameObject.SetActive(hasLevels);
            decreaseButton.interactable = viewed > 1;
            increaseButton.interactable = viewed < row.MaxViewedLevel;

            ShowAction(talent, viewed);
        }

        private void ShowAction(TalentDefinition talent, int viewed)
        {
            actionLabel.color = _actionColor;

            if (!Talents.IsUnlocked(_character, talent))
            {
                SetAction($"Unlocks at Mastery {talent.UnlockLevel}", false);
                return;
            }

            if (viewed <= Talents.Level(talent))
            {
                SetAction(IsEquippedAt(talent, viewed) ? "Equipped" : "Equip", true);
                return;
            }

            // Only the next level up can be viewed, so this is the one to buy
            var info = talent.LevelInfo(viewed);
            if (!Talents.CanUpgrade(_character, talent))
            {
                SetAction($"Unlocks at Mastery {info.MasteryLevel}", false);
                return;
            }

            SetAction($"{info.GloryCost} Glory", true);
            if (Glory.Balance < info.GloryCost) actionLabel.color = unaffordableColor;
        }

        private void SetAction(string text, bool interactable)
        {
            actionLabel.text = text;
            actionButton.interactable = interactable;
        }

        private void Begin() => _onBegin?.Invoke();
    }
}
