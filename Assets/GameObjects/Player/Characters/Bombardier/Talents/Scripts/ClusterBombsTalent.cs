using System.Collections;
using GameObjects.Common.Events;
using GameObjects.Common.Upgrades.Scripts;
using GameObjects.Common.Utils;
using GameObjects.Player.Shared.Scripts.Talents;
using UnityEngine;
using ExplosionEffect = GameObjects.Common.Effects.Explosion.Scripts.Explosion;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: every bomb scatters smaller blasts around where it went off. Level 2 makes them hit harder,
    // and from level 3 every point of Plenty (ProjectileCount) adds one more
    public class ClusterBombsTalent : MonoBehaviour, ILeveledTalent
    {
        [SerializeField] private ExplosionEffect explosionPrefab;
        [SerializeField, Min(1)] private int bomblets = 3;
        [Tooltip("How far out the small blasts land, as a multiple of the bomb's radius")]
        [SerializeField, Min(0f)] private float distance = 1f;
        [Tooltip("Damage of each small blast compared to the bomb, per talent level (level 1 first)")]
        [SerializeField] private float[] damageFactorPerLevel = { 0.5f, 0.75f, 0.75f };
        [Tooltip("Radius of each small blast compared to the bomb")]
        [SerializeField, Range(0.1f, 1f)] private float radiusFactor = 0.5f;
        [Tooltip("Seconds between the small blasts")]
        [SerializeField, Min(0f)] private float stagger = 0.1f;
        [Tooltip("From this talent level on, every point of Plenty adds one small blast")]
        [SerializeField, Min(1)] private int plentyFromLevel = 3;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;
        private int _level = 1;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()  => _events.OnBombExploded += Scatter;
        private void OnDisable() => _events.OnBombExploded -= Scatter;

        public void SetLevel(int level)
        {
            _level = level;
            if (_level < plentyFromLevel) return;

            // Plenty cards are only offered once something reads the stat
            var eligibility = GetComponentInParent<UpgradeEligibility>();
            if (eligibility != null) eligibility.Set(UpgradeRequirementType.ProjectileCount, true);
            else Debug.LogWarning("ClusterBombsTalent: No UpgradeEligibility on the player, so Plenty cards won't be offered.");
        }

        private int Count => _level >= plentyFromLevel
            ? bomblets + Mathf.Max(0, Mathf.RoundToInt(_stats.ProjectileCount.Value))
            : bomblets;

        private float DamageFactor => damageFactorPerLevel.Length == 0
            ? 0.5f
            : damageFactorPerLevel[Mathf.Clamp(_level - 1, 0, damageFactorPerLevel.Length - 1)];

        private void Scatter(Vector2 position, int damage, float radius) =>
            StartCoroutine(ScatterRoutine(position, damage, radius));

        private IEnumerator ScatterRoutine(Vector2 position, int damage, float radius)
        {
            int count = Count;
            float startAngle = Random.Range(0f, 360f);
            int smallDamage = Mathf.Max(1, Mathf.RoundToInt(damage * DamageFactor));
            var wait = stagger > 0f ? new WaitForSeconds(stagger) : null;

            for (int i = 0; i < count; i++)
            {
                yield return wait;
                float angle = (startAngle + 360f / count * i) * Mathf.Deg2Rad;
                var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * distance);
                var blast = PrefabPool.Spawn(explosionPrefab, position + offset, Quaternion.identity);
                blast.Init(smallDamage, radius * radiusFactor);
            }
        }
    }
}
