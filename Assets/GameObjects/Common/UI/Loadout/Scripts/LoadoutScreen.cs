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
    // character. Left/right browses a talent's levels, shown in the details box beside the list. Interact (or a click)
    // equips or unequips the talent, or buys the viewed level when it is the next one; equipping with every slot full
    // swaps out the oldest pick. The button on a row's right also buys the next level. Begin starts the run,
    // Back or Pause returns to the select screen. The loadout is saved per character
    public class LoadoutScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
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

        private readonly List<LoadoutRow> _rows = new();
        private List<TalentDefinition> _equipped;
        private MenuNavigator _navigator;
        private CharacterDefinition _character;
        private LoadoutRow _shownRow;
        private Action _onBegin;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            backButton.onClick.AddListener(Close);
            beginButton.onClick.AddListener(Begin);
            panel.SetActive(false);
        }

        public void Open(CharacterDefinition character, Action onBegin, Action onClosed)
        {
            _character = character;
            _onBegin = onBegin;
            _onClosed = onClosed;
            _equipped = Talents.Equipped(character);

            panel.SetActive(true);
            BuildRows();
            _shownRow = _rows.Count > 0 ? _rows[0] : null;
            detailsBox.SetActive(_shownRow != null);
            Refresh();
            _navigator.BlockInputThisFrame();
            // Begin first, so moving down past the last talent lands on Begin, and Begin is selected when there
            // are no talents yet
            _navigator.Select(beginButton);
            if (_rows.Count > 0) _navigator.Select(0, 0);
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen || _navigator.InputBlocked) return;

            if (GameInput.PausePressed)
            {
                Close();
                return;
            }

            _navigator.Navigate();

            if (_navigator.Row < _rows.Count)
            {
                if (GameInput.NavigateLeft)       _rows[_navigator.Row].ShowPreviousLevel();
                else if (GameInput.NavigateRight) _rows[_navigator.Row].ShowNextLevel();
            }

            if (GameInput.InteractPressed) _navigator.Submit();
        }

        // Rows depend on the character, so they are rebuilt on every open (a menu, not a per-frame path)
        private void BuildRows()
        {
            foreach (var row in _rows) Destroy(row.gameObject);
            _rows.Clear();

            var talents = _character.Talents;
            var items = new List<Button[]>();
            var start = ((RectTransform)rowTemplate.transform).anchoredPosition;

            for (int i = 0; i < talents.Count; i++)
            {
                if (talents[i] == null) continue;
                var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
                row.gameObject.SetActive(true);
                row.name = talents[i].name;
                ((RectTransform)row.transform).anchoredPosition = start + Vector2.down * (rowSpacing * _rows.Count);
                row.Bind(talents[i], Talents.Level(talents[i]));
                row.Button.onClick.AddListener(() => Press(row));
                row.UpgradeButton.onClick.AddListener(() => Upgrade(row));
                row.OnViewedLevelChanged += ShowDetails;
                _rows.Add(row);
                // One navigator row per talent, so left/right is free to browse levels
                items.Add(new[] { row.Button });
            }
            items.Add(new[] { backButton, beginButton });

            _navigator = new MenuNavigator(items.ToArray());
            _navigator.OnSelectionChanged += ShowDescription;
        }

        // Pressing a talent on the next level up buys that level; on a bought level it equips or unequips the talent
        private void Press(LoadoutRow row)
        {
            int level = Talents.Level(row.Talent);
            if (row.ViewedLevel == level + 1) Upgrade(row);
            else if (row.ViewedLevel <= level) Toggle(row.Talent);
        }

        private void Toggle(TalentDefinition talent)
        {
            if (!Talents.IsUnlocked(_character, talent)) return;

            if (_equipped.Contains(talent))
                _equipped.Remove(talent);
            else
            {
                if (_equipped.Count >= Talents.Slots(_character)) _equipped.RemoveAt(0);
                _equipped.Add(talent);
            }

            Talents.SetEquipped(_character, _equipped);
            Refresh();
        }

        private void Upgrade(LoadoutRow row)
        {
            if (!Talents.CanUpgrade(_character, row.Talent)) return;

            if (!Talents.TryUpgrade(_character, row.Talent))
            {
                ShowDetails(row);
                detailsBody.text += "\n\n<color=#B33A3A>Not enough Glory</color>";
                return;
            }

            row.View(Talents.Level(row.Talent));
            Refresh();
            // A bought-out or locked upgrade button can't stay selected after a click, so go back to the talent
            if (!row.UpgradeButton.gameObject.activeSelf || !row.UpgradeButton.interactable) _navigator.Select(row.Button);
        }

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
                row.Refresh(Talents.IsUnlocked(_character, row.Talent), _equipped.Contains(row.Talent), Talents.Level(row.Talent),
                            Talents.NextLevel(row.Talent), Talents.CanUpgrade(_character, row.Talent),
                            Glory.Balance >= (Talents.NextLevel(row.Talent)?.GloryCost ?? 0));

            if (_shownRow != null) ShowDetails(_shownRow);
            ShowDescription(_navigator.Row, _navigator.Column);
        }

        private void ShowDescription(int row, int column)
        {
            if (row < _rows.Count)
            {
                ShowDetails(_rows[row]);
                descriptionText.text = _rows[row].Talent.MaxLevel > 1 ? "Left / right to see each level" : string.Empty;
            }
            else if (_rows.Count == 0) descriptionText.text = "No talents for this character yet";
            else                       descriptionText.text = column == 1 ? "Start the run with this loadout" : string.Empty;
        }

        // Name, "Level 2 / 3", what the viewed level does and whether it is bought, buyable or still locked
        private void ShowDetails(LoadoutRow row)
        {
            _shownRow = row;
            var talent = row.Talent;
            int viewed = row.ViewedLevel;
            int bought = Talents.Level(talent);

            detailsTitle.text = talent.DisplayName;
            detailsLevel.text = $"Level {viewed} / {talent.MaxLevel}";
            detailsBody.text = $"{talent.DescriptionAt(viewed)}\n\n<size=85%>{LevelStatus(talent, viewed, bought)}</size>";
        }

        private string LevelStatus(TalentDefinition talent, int viewed, int bought)
        {
            if (!Talents.IsUnlocked(_character, talent)) return $"Unlocks at Mastery {talent.UnlockLevel}";
            if (viewed <= bought) return _equipped.Contains(talent) ? "Unlocked, equipped" : "Unlocked";

            var info = talent.LevelInfo(viewed);
            bool masteryReached = Mastery.Level(_character.name) >= info.MasteryLevel;
            // Only the next level can be viewed, so this is the one to buy
            return masteryReached
                ? $"Press to buy for {info.GloryCost} Glory"
                : $"Unlocks at Mastery {info.MasteryLevel}, then costs {info.GloryCost} Glory";
        }

        private void Begin() => _onBegin?.Invoke();
    }
}
