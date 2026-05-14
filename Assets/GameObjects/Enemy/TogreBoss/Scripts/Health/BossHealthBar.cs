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

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Initialize(ActorEvents events, string bossName)
        {
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
