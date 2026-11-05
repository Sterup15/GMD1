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
    // character; Interact (or a click) equips or unequips it, and equipping with every slot full swaps out the
    // oldest pick. Begin starts the run, Back or Pause returns to the select screen. The loadout is saved per character
    public class LoadoutScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [Tooltip("Inactive template, copied once per talent")]
        [SerializeField] private LoadoutRow rowTemplate;
        [Tooltip("Distance between rows, going down from the template's position")]
        [SerializeField] private float rowSpacing = 85f;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI slotsText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button beginButton;

        private readonly List<LoadoutRow> _rows = new();
        private List<TalentDefinition> _equipped;
        private MenuNavigator _navigator;
        private CharacterDefinition _character;
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
                row.Bind(talents[i]);
                var talent = talents[i];
                row.Button.onClick.AddListener(() => Toggle(talent));
                _rows.Add(row);
                items.Add(new[] { row.Button });
            }
            items.Add(new[] { backButton, beginButton });

            _navigator = new MenuNavigator(items.ToArray());
            _navigator.OnSelectionChanged += ShowDescription;
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

        private void Refresh()
        {
            int mastery = Mastery.Level(_character.name);
            int slots = Talents.SlotsFor(mastery);
            int nextSlot = Talents.NextSlotLevel(mastery);

            titleText.text = $"{_character.DisplayName} Talents";
            slotsText.text = $"Mastery {mastery}    Slots {_equipped.Count}/{slots}" +
                             (nextSlot > 0 ? $"  <size=75%>(next at Mastery {nextSlot})</size>" : string.Empty);

            foreach (var row in _rows)
                row.Refresh(Talents.IsUnlocked(_character, row.Talent), _equipped.Contains(row.Talent));

            ShowDescription(_navigator.Row, _navigator.Column);
        }

        private void ShowDescription(int row, int column)
        {
            if (row < _rows.Count)
            {
                var talent = _rows[row].Talent;
                descriptionText.text = Talents.IsUnlocked(_character, talent)
                    ? talent.Description
                    : $"{talent.Description}\n<size=80%>Unlocks at Mastery {talent.UnlockLevel}</size>";
            }
            else if (_rows.Count == 0) descriptionText.text = "No talents for this character yet";
            else                       descriptionText.text = column == 1 ? "Start the run with this loadout" : string.Empty;
        }

        private void Begin() => _onBegin?.Invoke();
    }
}
