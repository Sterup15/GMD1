using GameObjects.Player.Shared.Scripts.Talents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Loadout.Scripts
{
    // One talent row: "Blast Dash Lv 2      Equipped". The row is the button that gets selected; it remembers which
    // of the talent's levels the details box shows, limited to the bought levels and the next one
    [RequireComponent(typeof(Button))]
    public class LoadoutRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] private Color equippedColor = new(1f, 0.84f, 0.3f);
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color lockedColor = new(1f, 1f, 1f, 0.45f);

        public TalentDefinition Talent { get; private set; }
        public Button Button { get; private set; }
        public int ViewedLevel { get; private set; } = 1;
        public int MaxViewedLevel { get; private set; } = 1;

        private void Awake() => Button = GetComponent<Button>();

        public void Bind(TalentDefinition talent, int viewedLevel)
        {
            Talent = talent;
            ViewedLevel = viewedLevel;
        }

        // Returns whether the viewed level changed
        public bool View(int level)
        {
            level = Mathf.Clamp(level, 1, MaxViewedLevel);
            if (level == ViewedLevel) return false;
            ViewedLevel = level;
            return true;
        }

        // shownLevel is the equipped level while equipped, otherwise the highest bought one
        public void Refresh(bool unlocked, bool equipped, int boughtLevel, int shownLevel)
        {
            MaxViewedLevel = unlocked ? Mathf.Min(boughtLevel + 1, Talent.MaxLevel) : 1;
            View(ViewedLevel);

            if (!unlocked)
            {
                label.text = Talent.DisplayName;
                value.text = $"Mastery {Talent.UnlockLevel}";
                label.color = value.color = lockedColor;
                return;
            }

            label.text = Talent.MaxLevel > 1 ? $"{Talent.DisplayName} <size=70%>Lv {shownLevel}</size>" : Talent.DisplayName;
            value.text = equipped ? "Equipped" : string.Empty;
            label.color = value.color = equipped ? equippedColor : availableColor;
        }
    }
}
