using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Stats.Scripts
{
    public class Stats : MonoBehaviour
    {
        public Stat MoveSpeed;
        public Stat FireRate;
        public Stat Damage;
        public Stat MaxHealth;
        public Stat ShootRange;
        public Stat PickupRange;
        public Stat Bounce;
        public Stat Penetration;

        private bool _bounceUnlocked;

        private void Awake()
        {
            Bounce.OnValueChanged += OnBounceChanged;
            OnBounceChanged();
        }

        private void OnDestroy()
        {
            Bounce.OnValueChanged -= OnBounceChanged;
        }

        private void OnBounceChanged()
        {
            if (_bounceUnlocked || Bounce.Value < 1f) return;
            _bounceUnlocked = true;
            GlobalEvents.BounceUnlocked();
        }

        public Stat GetStat(StatType type) => type switch
        {
            StatType.MoveSpeed    => MoveSpeed,
            StatType.FireRate     => FireRate,
            StatType.Damage       => Damage,
            StatType.MaxHealth    => MaxHealth,
            StatType.ShootRange   => ShootRange,
            StatType.PickupRange  => PickupRange,
            StatType.Bounce       => Bounce,
            StatType.Penetration  => Penetration,
            _                     => null
        };
    }
}