using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.Armory.Scripts
{
    // One "Vitality  2/5      300" row. The row is the button; pressing it buys the next level
    [RequireComponent(typeof(Button))]
    public class ArmoryRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = new(1f, 0.45f, 0.4f);

        public ArmoryUpgrade Upgrade { get; private set; }
        public Button Button { get; private set; }

        private void Awake() => Button = GetComponent<Button>();

        public void Bind(ArmoryUpgrade upgrade) => Upgrade = upgrade;

        public void Refresh(int glory)
        {
            int level = Armory.Level(Upgrade);
            label.text = $"{Upgrade.DisplayName}  {level}/{Upgrade.MaxLevel}";

            if (Armory.IsMaxed(Upgrade))
            {
                value.text = "Max";
                value.color = affordableColor;
                return;
            }

            int cost = Armory.NextCost(Upgrade);
            value.text = cost.ToString();
            value.color = glory >= cost ? affordableColor : unaffordableColor;
        }
    }
}
