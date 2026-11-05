using GameObjects.Player.Shared.Scripts.Talents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Loadout.Scripts
{
    // One talent row: "Split Shot      Equipped". The row is the button; pressing it equips or unequips the talent
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

        private void Awake() => Button = GetComponent<Button>();

        public void Bind(TalentDefinition talent) => Talent = talent;

        public void Refresh(bool unlocked, bool equipped)
        {
            label.text = Talent.DisplayName;
            if (!unlocked)
            {
                value.text = $"Mastery {Talent.UnlockLevel}";
                label.color = value.color = lockedColor;
                return;
            }

            value.text = equipped ? "Equipped" : string.Empty;
            label.color = value.color = equipped ? equippedColor : availableColor;
        }
    }
}
