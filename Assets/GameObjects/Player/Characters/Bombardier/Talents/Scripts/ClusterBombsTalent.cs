using System.Collections;
using GameObjects.Common.Events;
using UnityEngine;
using ExplosionEffect = GameObjects.Common.Effects.Explosion.Scripts.Explosion;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: every bomb scatters smaller blasts around where it went off
    public class ClusterBombsTalent : MonoBehaviour
    {
        [SerializeField] private ExplosionEffect explosionPrefab;
        [SerializeField, Min(1)] private int bomblets = 3;
        [Tooltip("How far out the small blasts land, as a multiple of the bomb's radius")]
        [SerializeField, Min(0f)] private float distance = 1f;
        [Tooltip("Damage and radius of each small blast compared to the bomb")]
        [SerializeField, Range(0.1f, 1f)] private float damageFactor = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float radiusFactor = 0.5f;
        [Tooltip("Seconds between the small blasts")]
        [SerializeField, Min(0f)] private float stagger = 0.1f;

        private ActorEvents _events;

        private void Awake() => _events = GetComponentInParent<ActorEvents>();

        private void OnEnable()  => _events.OnBombExploded += Scatter;
        private void OnDisable() => _events.OnBombExploded -= Scatter;

        private void Scatter(Vector2 position, int damage, float radius) =>
            StartCoroutine(ScatterRoutine(position, damage, radius));

        private IEnumerator ScatterRoutine(Vector2 position, int damage, float radius)
        {
            float startAngle = Random.Range(0f, 360f);
            int smallDamage = Mathf.Max(1, Mathf.RoundToInt(damage * damageFactor));
            var wait = stagger > 0f ? new WaitForSeconds(stagger) : null;

            for (int i = 0; i < bomblets; i++)
            {
                yield return wait;
                float angle = (startAngle + 360f / bomblets * i) * Mathf.Deg2Rad;
                var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * distance);
                var blast = Instantiate(explosionPrefab, position + offset, Quaternion.identity);
                blast.Init(smallDamage, radius * radiusFactor);
            }
        }
    }
}
