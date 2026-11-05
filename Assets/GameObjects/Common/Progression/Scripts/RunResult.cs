namespace GameObjects.Common.Progression.Scripts
{
    // What a finished run earned, sent with GlobalEvents.OnRunEnded for the results screen
    public struct RunResult
    {
        public bool Won;
        public int Kills;
        public float Seconds;
        public int KillPoints;
        public int TimePoints;
        public int VictoryPoints;
        public int GloryEarned;
        public int GloryTotal;
        // Empty when no character is known (then no mastery XP is given)
        public string CharacterName;
        public int MasteryXpEarned;
        public int MasteryLevelBefore;
        public int MasteryLevelAfter;

        public int Points => KillPoints + TimePoints + VictoryPoints;
    }
}
