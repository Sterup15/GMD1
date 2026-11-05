using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.Upgrades.Scripts
{
    public class UpgradeCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button button;
        [Tooltip("Soft glow behind the card, tinted by rarity")]
        [SerializeField] private Image glow;
        [Tooltip("Glow colour per rarity, in UpgradeRarity order (Common, Uncommon, Rare, Legendary). Alpha is the strength")]
        [SerializeField] private Color[] rarityGlow =
        {
            new(1f, 1f, 1f, 0f),
            new(0.35f, 0.9f, 0.35f, 0.3f),
            new(0.3f, 0.6f, 1f, 0.6f),
            new(1f, 0.6f, 0.15f, 0.9f),
        };

        private Action _onPicked;

        public Button Button => button;

        public void Setup(UpgradeDefinition definition, Action onPicked)
        {
            titleText.text = definition.displayName;
            descriptionText.text = definition.description;
            _onPicked = onPicked;

            int rarity = (int)definition.rarity;
            if (glow != null && rarity < rarityGlow.Length)
                glow.color = rarityGlow[rarity];

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onPicked?.Invoke());
        }
    }
}
