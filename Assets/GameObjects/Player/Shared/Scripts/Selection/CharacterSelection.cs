namespace GameObjects.Player.Shared.Scripts.Selection
{
    // The character picked in the main menu. Static so it survives the scene load and restarts
    public static class CharacterSelection
    {
        public static int SelectedIndex { get; set; }
    }
}
