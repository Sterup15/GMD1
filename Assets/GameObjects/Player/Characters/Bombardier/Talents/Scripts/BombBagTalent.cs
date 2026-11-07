using GameObjects.Common.Events;
using GameObjects.Common.Upgrades.Scripts;
using GameObjects.Player.Shared.Scripts.Talents;
using UnityEngine;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: while not attacking, bombs pile up in a bag, at half the attack speed at level 1 and at the full
    // attack speed from level 2, whether standing still or running. An attack that starts with bombs in the bag throws
    // them much faster until the bag is empty. While attacking normally the bar shows the next throw coming instead.
    // Level 3 holds one more bomb plus one per point of Plenty
    public class BombBagTalent : MonoBehaviour, ILeveledTalent
    {
        [Tooltip("Bombs the bag holds at each talent level, level 1 first")]
        [SerializeField] private int[] capacityPerLevel = { 3, 3, 4 };
        [Tooltip("Bag filling speed while not attacking, compared to the attack speed, at each talent level")]
        [SerializeField] private float[] fillRatePerLevel = { 0.5f, 1f, 1f };
        [Tooltip("From this talent level on, every point of Plenty makes room for one more bomb")]
        [SerializeField, Min(1)] private int plentyFromLevel = 3;
        [Tooltip("Attack speed multiplier while emptying bombs saved up before the attack")]
        [SerializeField, Min(1f)] private float burstSpeedMultiplier = 10f;
        [Tooltip("Seconds of one attack at attack speed 1 (the Attack clip's length), so a fill rate of 1 is one bomb per attack")]
        [SerializeField, Min(0.01f)] private float attackCycle = 0.85f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;
        private int _level = 1;
        private bool _attacking;
        private float _progress;
        private float _burstBonus;

        public int Bombs { get; private set; }

        public int Capacity
        {
            get
            {
                int capacity = capacityPerLevel.Length == 0
                    ? 3
                    : capacityPerLevel[Mathf.Clamp(_level - 1, 0, capacityPerLevel.Length - 1)];
                if (_level >= plentyFromLevel)
                    capacity += Mathf.Max(0, Mathf.RoundToInt(_stats.ProjectileCount.Value));
                return capacity;
            }
        }

        // How far the next bomb is, 0 to 1. A full bag shows as full
        public float Progress => Bombs >= Capacity ? 1f : _progress;

        private float FillRate => fillRatePerLevel.Length == 0
            ? 0.5f
            : fillRatePerLevel[Mathf.Clamp(_level - 1, 0, fillRatePerLevel.Length - 1)];

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()
        {
            _events.OnAttackingChanged += SetAttacking;
            _events.OnShotFired        += UseBomb;
        }

        private void OnDisable()
        {
            _events.OnAttackingChanged -= SetAttacking;
            _events.OnShotFired        -= UseBomb;
            EndBurst();
        }

        public void SetLevel(int level)
        {
            _level = level;
            if (_level < plentyFromLevel) return;

            // Plenty cards are only offered once something reads the stat
            var eligibility = GetComponentInParent<UpgradeEligibility>();
            if (eligibility != null) eligibility.Set(UpgradeRequirementType.ProjectileCount, true);
            else Debug.LogWarning("BombBagTalent: No UpgradeEligibility on the player, so Plenty cards won't be offered.");
        }

        private void Update()
        {
            int capacity = Capacity;
            if (Bombs >= capacity) return;

            // Follows the normal attack speed, not the burst's
            float attacksPerSecond = (_stats.FireRate.Value - _burstBonus) / attackCycle;

            // While attacking, the bar is the bomb in hand: it keeps time with the attack and waits full for the throw
            // instead of going into the bag
            if (_attacking)
            {
                _progress = Mathf.Min(_progress + attacksPerSecond * Time.deltaTime, 1f);
                return;
            }

            _progress += attacksPerSecond * FillRate * Time.deltaTime;
            if (_progress < 1f) return;
            _progress -= 1f;
            Bombs++;
            if (Bombs >= capacity) _progress = 0f;
        }

        // Only bombs saved up before the attack started give a burst; normal attacks never add to the bag
        private void SetAttacking(bool attacking)
        {
            _attacking = attacking;
            if (attacking && Bombs > 0) StartBurst();
            else EndBurst();
        }

        // A burst throws from the bag; a normal throw throws the bomb the bar was filling, so the bar starts over and
        // keeps time with the attack
        private void UseBomb()
        {
            if (Bombs > 0)
            {
                Bombs--;
                if (Bombs == 0) EndBurst();
                return;
            }
            _progress = 0f;
        }

        private void StartBurst()
        {
            if (_burstBonus > 0f) return;
            _burstBonus = _stats.FireRate.Value * (burstSpeedMultiplier - 1f);
            if (_burstBonus > 0f) _stats.FireRate.AddBonus(_burstBonus);
        }

        private void EndBurst()
        {
            if (_burstBonus <= 0f) return;
            _stats.FireRate.RemoveBonus(_burstBonus);
            _burstBonus = 0f;
        }
    }
}
