using GameObjects.Common.Events;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Enemy.Common.Scripts.Health
{
    // The small health bar above an enemy. Follows OnHealthChanged and hides when the enemy runs off at the boss
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()
        {
            _events.OnHealthChanged += ShowHealth;
            _events.OnRetreat       += Hide;
        }

        private void OnDisable()
        {
            _events.OnHealthChanged -= ShowHealth;
            _events.OnRetreat       -= Hide;
        }

        private void ShowHealth(int current, int max)
        {
            if (slider != null)
                slider.value = max > 0 ? (float)current / max : 0f;
        }

        private void Hide(Vector2 _)
        {
            if (slider != null)
                slider.gameObject.SetActive(false);
        }
    }
}
