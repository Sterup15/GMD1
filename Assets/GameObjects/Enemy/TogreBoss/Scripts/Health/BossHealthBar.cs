using GameObjects.Common.Events;
using GameObjects.Enemy.Common.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Enemy.TogreBoss.Scripts.Health
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text nameText;

        private ActorEvents _events;

        // Subscribed in Awake/OnDestroy rather than OnEnable/OnDisable, because the bar hides itself
        // (inactive) between bosses and must still hear the next one spawn
        private void Awake()
        {
            GlobalEvents.OnBossSpawnedWithBoss += Show;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            GlobalEvents.OnBossSpawnedWithBoss -= Show;
            if (_events != null) _events.OnHealthChanged -= OnHealthChanged;
        }

        private void Show(ActorEvents events, string bossName)
        {
            if (events == null) return;
            if (_events != null)
                _events.OnHealthChanged -= OnHealthChanged;

            _events = events;
            _events.OnHealthChanged += OnHealthChanged;
            slider.value = 1f;

            if (nameText != null)
                nameText.text = bossName;

            gameObject.SetActive(true);
        }

        private void OnHealthChanged(int current, int max)
        {
            slider.value = (float)current / max;

            if (current == 0)
            {
                _events.OnHealthChanged -= OnHealthChanged;
                _events = null;
                gameObject.SetActive(false);
            }
        }
    }
}
