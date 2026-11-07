using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Progression.Scripts
{
    // Counts what the run earns (kills, time survived, winning) and, when the run ends, banks it as Glory and as
    // mastery XP for the character that was played, then raises GlobalEvents.OnRunEnded with the breakdown.
    // A defeat keeps everything earned so far
    public class RunPoints : MonoBehaviour
    {
        [Header("Points")]
        [Tooltip("Points for every enemy killed, the boss included")]
        [SerializeField, Min(0)] private int pointsPerKill = 1;
        [Tooltip("Points per minute survived, counted pro rata (paused time doesn't count)")]
        [SerializeField, Min(0)] private int pointsPerMinute = 10;
        [Tooltip("Extra points for winning the run")]
        [SerializeField, Min(0)] private int victoryBonus = 100;
        [Header("Conversion")]
        [Tooltip("Glory banked per run point when the run ends")]
        [SerializeField, Min(0f)] private float gloryPerPoint = 1f;
        [Tooltip("Mastery XP for the played character per run point when the run ends")]
        [SerializeField, Min(0f)] private float masteryXpPerPoint = 1f;

        private int _kills;
        private float _seconds;
        private bool _ended;

        private void OnEnable()
        {
            GlobalEvents.OnEnemyKilled += CountKill;
            GlobalEvents.OnPlayerDied  += EndAsDefeat;
            GlobalEvents.OnPlayerWon   += EndAsVictory;
        }

        private void OnDisable()
        {
            GlobalEvents.OnEnemyKilled -= CountKill;
            GlobalEvents.OnPlayerDied  -= EndAsDefeat;
            GlobalEvents.OnPlayerWon   -= EndAsVictory;
        }

        private void Update()
        {
            if (!_ended) _seconds += Time.deltaTime;
        }

        private void CountKill()
        {
            if (!_ended) _kills++;
        }

        private void EndAsDefeat()  => End(false);
        private void EndAsVictory() => End(true);

        private void End(bool won)
        {
            if (_ended) return;
            _ended = true;

            int killPoints = _kills * pointsPerKill;
            int timePoints = Mathf.FloorToInt(_seconds / 60f * pointsPerMinute);
            int victoryPoints = won ? victoryBonus : 0;
            int points = killPoints + timePoints + victoryPoints;

            int glory = Mathf.RoundToInt(points * gloryPerPoint);
            Glory.Add(glory);

            var result = new RunResult
            {
                Won = won,
                Kills = _kills,
                Seconds = _seconds,
                KillPoints = killPoints,
                TimePoints = timePoints,
                VictoryPoints = victoryPoints,
                GloryEarned = glory,
                GloryTotal = Glory.Balance,
            };

            var player = Players.Local;
            if (player != null && player.TryGetComponent<IMasteryCharacter>(out var character) && character.MasteryId.Length > 0)
            {
                result.CharacterName = character.DisplayName;
                result.MasteryLevelBefore = Mastery.Level(character.MasteryId);
                result.MasteryXpEarned = Mathf.RoundToInt(points * masteryXpPerPoint);
                Mastery.Add(character.MasteryId, result.MasteryXpEarned);
                result.MasteryLevelAfter = Mastery.Level(character.MasteryId);
            }

            GlobalEvents.RunEnded(result);
        }
    }
}
