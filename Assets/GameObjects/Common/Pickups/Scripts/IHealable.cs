namespace GameObjects.Common.Pickups.Scripts
{
    // Something a healing pickup can heal (the player's health)
    public interface IHealable
    {
        bool IsFullHealth { get; }
        void Heal(int amount);
    }
}
