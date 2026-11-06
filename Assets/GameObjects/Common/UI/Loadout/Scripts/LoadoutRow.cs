using System;
using GameObjects.Player.Shared.Scripts.Talents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Loadout.Scripts
{
    // One talent row: "< Blast Dash Lv 1 >   Equipped   [1500 Glory]". The row is the button that gets selected.
    // Left/right (or tapping the arrows) browses the talent's levels for the details box; the button on the right
    // buys the next level, or says which mastery level unlocks it
    [RequireComponent(typeof(Button))]
    public class LoadoutRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeLabel;
        [SerializeField] private Color equippedColor = new(1f, 0.84f, 0.3f);
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color lockedColor = new(1f, 1f, 1f, 0.45f);
        [Tooltip("Price colour when the player has too little Glory")]
        [SerializeField] private Color unaffordableColor = new(0.7f, 0.23f, 0.23f);

        public TalentDefinition Talent { get; private set; }
        public Button Button { get; private set; }
        public Button UpgradeButton => upgradeButton;
        // The level shown in the details box, which isn't necessarily the bought one
        public int ViewedLevel { get; private set; } = 1;

        public event Action<LoadoutRow> OnViewedLevelChanged;

        private Color _priceColor;
        // Only bought levels and the next one can be browsed
        private int _maxViewedLevel = 1;

        private void Awake()
        {
            Button = GetComponent<Button>();
            _priceColor = upgradeLabel.color;
            decreaseButton.onClick.AddListener(ShowPreviousLevel);
            increaseButton.onClick.AddListener(ShowNextLevel);
        }

        public void Bind(TalentDefinition talent, int viewedLevel)
        {
            Talent = talent;
            ViewedLevel = viewedLevel;
            bool hasLevels = talent.MaxLevel > 1;
            decreaseButton.gameObject.SetActive(hasLevels);
            increaseButton.gameObject.SetActive(hasLevels);
        }

        public void ShowPreviousLevel() => View(ViewedLevel - 1);
        public void ShowNextLevel()     => View(ViewedLevel + 1);

        public void View(int level)
        {
            level = Mathf.Clamp(level, 1, _maxViewedLevel);
            if (level == ViewedLevel) return;
            ViewedLevel = level;
            OnViewedLevelChanged?.Invoke(this);
        }

        // nextLevel is the level after the bought one (null at the max level); buyable says whether mastery allows it,
        // affordable whether the player has the Glory for it
        public void Refresh(bool unlocked, bool equipped, int level, TalentLevel nextLevel, bool buyable, bool affordable)
        {
            _maxViewedLevel = unlocked ? Mathf.Min(level + 1, Talent.MaxLevel) : 1;
            View(ViewedLevel);

            upgradeButton.gameObject.SetActive(unlocked && nextLevel != null);
            if (unlocked && nextLevel != null)
            {
                upgradeButton.interactable = buyable;
                upgradeLabel.text = buyable ? $"{nextLevel.GloryCost} Glory" : $"Unlocks at Mastery {nextLevel.MasteryLevel}";
                upgradeLabel.color = buyable && !affordable ? unaffordableColor : _priceColor;
            }

            if (!unlocked)
            {
                label.text = Talent.DisplayName;
                value.text = $"Mastery {Talent.UnlockLevel}";
                label.color = value.color = lockedColor;
                return;
            }

            label.text = Talent.MaxLevel > 1 ? $"{Talent.DisplayName} <size=70%>Lv {level}</size>" : Talent.DisplayName;
            value.text = equipped ? "Equipped" : string.Empty;
            label.color = value.color = equipped ? equippedColor : availableColor;
        }
    }
}
