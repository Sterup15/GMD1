using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Progression.Scripts
{
    // Counts what the run earns (kills, time survived, winning) and, when the run ends, banks it as Glory and
    // raises GlobalEvents.OnRunEnded with the breakdown. A defeat keeps everything earned so far
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
            int glory = Mathf.RoundToInt((killPoints + timePoints + victoryPoints) * gloryPerPoint);

            Glory.Add(glory);
            GlobalEvents.RunEnded(new RunResult(won, _kills, _seconds, killPoints, timePoints, victoryPoints,
                                                glory, Glory.Balance));
        }
    }
}
