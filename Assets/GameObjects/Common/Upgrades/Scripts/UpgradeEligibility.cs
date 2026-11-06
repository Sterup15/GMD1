using UnityEngine;

namespace GameObjects.Common.Upgrades.Scripts
{
    // Which kinds of upgrade cards the player can be offered. Set per character in the Inspector,
    // and changeable mid-run (e.g. an item that adds a projectile or explosive attack)
    public class UpgradeEligibility : MonoBehaviour
    {
        [Tooltip("Bounce and penetration cards")]
        [SerializeField] private bool projectiles;
        [Tooltip("Explosion area cards")]
        [SerializeField] private bool explosions;
        [Tooltip("Plenty cards. Normally off; switched on by talents that read ProjectileCount")]
        [SerializeField] private bool projectileCount;

        public bool Allows(UpgradeRequirementType requirement) => requirement switch
        {
            UpgradeRequirementType.Projectiles => projectiles,
            UpgradeRequirementType.Explosions  => explosions,
            UpgradeRequirementType.ProjectileCount => projectileCount,
            _                                  => true
        };

        public void Set(UpgradeRequirementType requirement, bool enabled)
        {
            switch (requirement)
            {
                case UpgradeRequirementType.Projectiles: projectiles = enabled; break;
                case UpgradeRequirementType.Explosions:  explosions  = enabled; break;
                case UpgradeRequirementType.ProjectileCount: projectileCount = enabled; break;
            }
        }
    }
}
