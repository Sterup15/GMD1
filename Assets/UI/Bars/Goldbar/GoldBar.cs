using Actors.Player.Scripts.Gold;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Bars.Goldbar
{
    public class GoldBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI levelText; // optional

        private void OnEnable()  => PlayerGold.OnGoldChanged += OnGoldChanged;
        private void OnDisable() => PlayerGold.OnGoldChanged -= OnGoldChanged;

        private void Start() => OnGoldChanged(0, 1);

        private void OnGoldChanged(int current, int threshold)
        {
            slider.value = (float)current / threshold;
            if (levelText != null)
                levelText.text = $"Lvl {FindFirstObjectByType<PlayerGold>()?.Level ?? 0}";
        }
    }
}