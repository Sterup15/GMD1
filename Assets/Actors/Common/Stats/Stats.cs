using UnityEngine;

namespace Actors.Common
{
    public class Stats : MonoBehaviour
    {
        public Stat MoveSpeed;
        public Stat FireRate;
        public Stat Damage;
        public Stat MaxHealth;
        public Stat ShootRange;
        public Stat PickupRange;

        public Stat GetStat(StatType type) => type switch
        {
            StatType.MoveSpeed   => MoveSpeed,
            StatType.FireRate    => FireRate,
            StatType.Damage      => Damage,
            StatType.MaxHealth   => MaxHealth,
            StatType.ShootRange  => ShootRange,
            StatType.PickupRange => PickupRange,
            _                    => null
        };
    }
}