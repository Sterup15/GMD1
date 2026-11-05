using GameObjects.Common.Events;
using UnityEngine;
using ExplosionEffect = GameObjects.Common.Effects.Explosion.Scripts.Explosion;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: dashing leaves an explosion where the dash started, hitting enemies that crowded in
    public class BlastDashTalent : MonoBehaviour
    {
        [SerializeField] private ExplosionEffect explosionPrefab;
        [Tooltip("Blast radius before the Area stat")]
        [SerializeField, Min(0.1f)] private float radius = 1.2f;
        [Tooltip("Damage as a multiple of the Damage stat")]
        [SerializeField, Min(0f)] private float damageFactor = 1f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()  => _events.OnDashStarted += Blast;
        private void OnDisable() => _events.OnDashStarted -= Blast;

        private void Blast(Vector2 _)
        {
            var blast = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            blast.Init(Mathf.Max(1, Mathf.RoundToInt(_stats.Damage.Value * damageFactor)), radius * _stats.Area.Value);
        }
    }
}
