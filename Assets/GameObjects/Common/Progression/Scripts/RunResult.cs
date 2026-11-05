namespace GameObjects.Common.Progression.Scripts
{
    // What a finished run earned, sent with GlobalEvents.OnRunEnded for the results screen
    public readonly struct RunResult
    {
        public readonly bool Won;
        public readonly int Kills;
        public readonly float Seconds;
        public readonly int KillPoints;
        public readonly int TimePoints;
        public readonly int VictoryPoints;
        public readonly int GloryEarned;
        public readonly int GloryTotal;

        public int Points => KillPoints + TimePoints + VictoryPoints;

        public RunResult(bool won, int kills, float seconds, int killPoints, int timePoints, int victoryPoints,
                         int gloryEarned, int gloryTotal)
        {
            Won = won;
            Kills = kills;
            Seconds = seconds;
            KillPoints = killPoints;
            TimePoints = timePoints;
            VictoryPoints = victoryPoints;
            GloryEarned = gloryEarned;
            GloryTotal = gloryTotal;
        }
    }
}
