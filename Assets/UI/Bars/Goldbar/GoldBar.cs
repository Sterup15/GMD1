using GameObjects.Common.Progression.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Bars.Goldbar
{
    // The team's gold towards the next level, and the level, from the scene's TeamLevel
    public class GoldBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI levelText; // optional

        private TeamLevel _team;

        private void Start()
        {
            OnGoldChanged(0, 1, 0);

            _team = TeamLevel.Current;
            if (_team == null)
            {
                Debug.LogWarning("GoldBar: No TeamLevel in the scene.", this);
                return;
            }

            _team.OnGoldChanged += OnGoldChanged;
            OnGoldChanged(_team.Gold, _team.Threshold, _team.Level);
        }

        // TeamLevel is only found in Start, so the first subscription happens there
        private void OnEnable()
        {
            if (_team != null) _team.OnGoldChanged += OnGoldChanged;
        }

        private void OnDisable()
        {
            if (_team != null) _team.OnGoldChanged -= OnGoldChanged;
        }

        private void OnGoldChanged(int current, int threshold, int level)
        {
            slider.value = (float)current / threshold;
            if (levelText != null)
                levelText.text = $"Lvl {level}";
        }
    }
}
