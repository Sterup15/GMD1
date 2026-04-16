using System;
using Actors.Common.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Upgrade
{
    public class UpgradeCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button button;

        public void Setup(UpgradeDefinition definition, Action onPicked)
        {
            titleText.text = definition.displayName;
            descriptionText.text = definition.description;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onPicked?.Invoke());
        }
    }
}
