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
    public class ArmoryScreen : MenuScreen
    {
        [Tooltip("Inactive template, copied once per upgrade")]
        [SerializeField] private ArmoryRow rowTemplate;
        [Tooltip("Distance between rows, going down from the template's position")]
        [SerializeField] private float rowSpacing = 85f;
        [SerializeField] private TextMeshProUGUI gloryText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button refundButton;
        [SerializeField] private Button backButton;

        private ArmoryRow[] _rows;

        private void Awake()
        {
            var upgrades = Armory.Upgrades;
            _rows = new ArmoryRow[upgrades.Count];
            var items = new Button[upgrades.Count + 1][];

            for (int i = 0; i < upgrades.Count; i++)
            {
                var row = CreateRow(rowTemplate, i, rowSpacing);
                row.name = upgrades[i].name;
                row.Bind(upgrades[i]);
                var upgrade = upgrades[i];
                row.Button.onClick.AddListener(() => Buy(upgrade));
                _rows[i] = row;
                items[i] = new[] { row.Button };
            }
            items[upgrades.Count] = new[] { refundButton, backButton };

            Navigator = new MenuNavigator(items);
            Navigator.OnSelectionChanged += ShowDescription;
            refundButton.onClick.AddListener(Refund);
            backButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            ShowPanel(onClosed);
            Refresh();
            // Back first, so moving down past the last upgrade lands on Back rather than Refund
            Navigator.Select(backButton);
            Navigator.Select(0, 0);
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
            ShowDescription(Navigator.Row, Navigator.Column);
        }

        private void ShowDescription(int row, int column)
        {
            if (row < _rows.Length)            descriptionText.text = _rows[row].Upgrade.Description;
            else if (column == 0)              descriptionText.text = "Get back all the Glory spent here, to spend it differently";
            else                               descriptionText.text = string.Empty;
        }
    }
}
