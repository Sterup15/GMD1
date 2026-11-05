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
        public Stat Haste;
        public Stat DashCharges;
        public Stat Area;
        public Stat Luck;

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
            StatType.Haste        => Haste,
            StatType.DashCharges  => DashCharges,
            StatType.Area         => Area,
            StatType.Luck         => Luck,
            _                     => null
        };
    }
}