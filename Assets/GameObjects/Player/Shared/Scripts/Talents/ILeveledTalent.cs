namespace GameObjects.Player.Shared.Scripts.Talents
{
    // Implemented by talent behaviour scripts that change with the talent's level. Talents.Apply calls it right
    // after spawning the talent prefab, before its first frame
    public interface ILeveledTalent
    {
        void SetLevel(int level);
    }
}
