namespace GameObjects.Common.Progression.Scripts
{
    // The character a player is playing, as far as run rewards need it: Mastery is saved under MasteryId.
    // Found on the player (PlayedCharacter), so every player banks mastery for their own character
    public interface IMasteryCharacter
    {
        string MasteryId { get; }
        string DisplayName { get; }
    }
}
