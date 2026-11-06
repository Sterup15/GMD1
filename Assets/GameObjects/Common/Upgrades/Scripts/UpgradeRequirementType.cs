namespace GameObjects.Common.Upgrades.Scripts
{
    // What a character needs for an upgrade card to be useful. Checked against UpgradeEligibility on the player
    public enum UpgradeRequirementType
    {
        None,
        Projectiles,
        Explosions,
        // A talent that reads ProjectileCount (Plenty) is equipped at a level that uses it
        ProjectileCount
    }
}
