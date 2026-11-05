using System;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Common.UI.Navigation.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.Armory.Scripts
{
    // The Armory: spend Glory on permanent upgrades for every character. One row per ArmoryUpgrade asset, built
    // when the screen is created, so a new upgrade needs no UI work. Up/down picks a row, Interact (or a click)
    // buys its next level, Refund gives all spent Glory back, and Pause or Back closes the screen
    public class ArmoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [Tooltip("Inactive template, copied once per upgrade")]
        [SerializeField] private ArmoryRow rowTemplate;
        [Tooltip("Distance between rows, going down from the template's position")]
        [SerializeField] private float rowSpacing = 85f;
        [SerializeField] private TextMeshProUGUI gloryText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button refundButton;
        [SerializeField] private Button backButton;

        private ArmoryRow[] _rows;
        private MenuNavigator _navigator;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            var upgrades = Armory.Upgrades;
            _rows = new ArmoryRow[upgrades.Count];
            var items = new Button[upgrades.Count + 1][];
            var start = ((RectTransform)rowTemplate.transform).anchoredPosition;

            for (int i = 0; i < upgrades.Count; i++)
            {
                var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
                row.gameObject.SetActive(true);
                row.name = upgrades[i].name;
                ((RectTransform)row.transform).anchoredPosition = start + Vector2.down * (rowSpacing * i);
                row.Bind(upgrades[i]);
                var upgrade = upgrades[i];
                row.Button.onClick.AddListener(() => Buy(upgrade));
                _rows[i] = row;
                items[i] = new[] { row.Button };
            }
            items[upgrades.Count] = new[] { refundButton, backButton };

            _navigator = new MenuNavigator(items);
            _navigator.OnSelectionChanged += ShowDescription;
            refundButton.onClick.AddListener(Refund);
            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            panel.SetActive(true);
            Refresh();
            _navigator.BlockInputThisFrame();
            // Back first, so moving down past the last upgrade lands on Back rather than Refund
            _navigator.Select(backButton);
            _navigator.Select(0, 0);
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

        private void Buy(ArmoryUpgrade upgrade)
        {
            if (Armory.TryBuy(upgrade)) Refresh();
        }

        private void Refund()
        {
            if (Armory.RefundAll() > 0) Refresh();
        }

        private void Refresh()
        {
            int glory = Glory.Balance;
            gloryText.text = $"Glory: {glory}";
            foreach (var row in _rows) row.Refresh(glory);
            ShowDescription(_navigator.Row, _navigator.Column);
        }

        private void ShowDescription(int row, int column)
        {
            if (row < _rows.Length)            descriptionText.text = _rows[row].Upgrade.Description;
            else if (column == 0)              descriptionText.text = "Get back all the Glory spent here, to spend it differently";
            else                               descriptionText.text = string.Empty;
        }
    }
}
