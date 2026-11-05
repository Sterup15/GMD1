using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Bars.Goldbar
{
    // Gold towards the next level, and the level. Listens to the player's own ActorEvents, found by tag when the run starts
    public class GoldBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI levelText; // optional

        private ActorEvents _events;

        private void Start()
        {
            OnGoldChanged(0, 1, 0);

            var player = Players.Local;
            if (player == null)
            {
                Debug.LogWarning("GoldBar: No local player registered.", this);
                return;
            }

            _events = player.GetComponent<ActorEvents>();
            _events.OnGoldChanged += OnGoldChanged;
        }

        // The player is only found in Start, so the first subscription happens there
        private void OnEnable()
        {
            if (_events != null) _events.OnGoldChanged += OnGoldChanged;
        }

        private void OnDisable()
        {
            if (_events != null) _events.OnGoldChanged -= OnGoldChanged;
        }

        private void OnGoldChanged(int current, int threshold, int level)
        {
            slider.value = (float)current / threshold;
            if (levelText != null)
                levelText.text = $"Lvl {level}";
        }
    }
}
