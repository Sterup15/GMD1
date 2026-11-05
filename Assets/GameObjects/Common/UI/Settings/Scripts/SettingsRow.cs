using GameObjects.Common.Settings.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Settings.Scripts
{
    // One "Music  < 80% >" row. Left/right (or tapping the arrows) changes the setting; the row itself is the
    // button that gets selected, and pressing it toggles on/off settings such as fullscreen
    [RequireComponent(typeof(Button))]
    public class SettingsRow : MonoBehaviour
    {
        [SerializeField] private SettingType setting;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [Tooltip("How much one press changes a volume (0.1 = 10%)")]
        [SerializeField, Range(0.01f, 0.5f)] private float step = 0.1f;

        private bool IsToggle => setting == SettingType.Fullscreen;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Press);
            decreaseButton.onClick.AddListener(Decrease);
            increaseButton.onClick.AddListener(Increase);
        }

        private void OnEnable() => Show(GameSettings.Get(setting));

        public void Decrease() => Change(-1);
        public void Increase() => Change(1);

        private void Press()
        {
            if (IsToggle) Change(1);
        }

        private void Change(int direction)
        {
            float value = IsToggle
                ? 1f - GameSettings.Get(setting)
                : Mathf.Clamp01(Mathf.Round((GameSettings.Get(setting) + direction * step) * 100f) / 100f);

            GameSettings.Set(setting, value);
            // Show the new value directly, because the window mode only switches on the next frame
            Show(value);
        }

        private void Show(float value) =>
            valueText.text = IsToggle ? (value > 0.5f ? "On" : "Off") : $"{Mathf.RoundToInt(value * 100f)}%";
    }
}
