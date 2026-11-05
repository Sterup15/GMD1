using System.Collections;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Player.Characters.Archer.Talents.Scripts
{
    // Archer talent: attack faster for a few seconds after every dash. Dashing again restarts the timer
    public class AdrenalineTalent : MonoBehaviour
    {
        [Tooltip("Extra attack speed as a percentage of the character's base attack speed")]
        [SerializeField, Min(0f)] private float attackSpeedPercent = 50f;
        [SerializeField, Min(0f)] private float duration = 3f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;
        private Coroutine _boost;
        private float _bonus;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable() => _events.OnDashStarted += StartBoost;

        private void OnDisable()
        {
            _events.OnDashStarted -= StartBoost;
            EndBoost();
        }

        private void StartBoost(Vector2 _)
        {
            if (_boost != null) StopCoroutine(_boost);
            else
            {
                _bonus = _stats.FireRate.BaseValue * attackSpeedPercent / 100f;
                _stats.FireRate.AddBonus(_bonus);
            }
            _boost = StartCoroutine(BoostRoutine());
        }

        private IEnumerator BoostRoutine()
        {
            yield return new WaitForSeconds(duration);
            _boost = null;
            _stats.FireRate.RemoveBonus(_bonus);
        }

        private void EndBoost()
        {
            if (_boost == null) return;
            StopCoroutine(_boost);
            _boost = null;
            _stats.FireRate.RemoveBonus(_bonus);
        }
    }
}
