using System;
using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Progression.Scripts
{
    // The run's gold and level, shared by the whole team: every player's gold fills one bar, and each level-up raises
    // GlobalEvents.OnLevelUp (each player then picks their own card). One object in the game scene; pickups and the
    // gold bar find it through Current
    public class TeamLevel : MonoBehaviour
    {
        [Tooltip("X = team level, Y = gold needed to reach the next level")]
        [SerializeField] private AnimationCurve goldThresholdCurve;
        [Tooltip("Extra gold needed per level, on top of the curve")]
        [SerializeField] private int thresholdBonusPerLevel = 2;

        public static TeamLevel Current { get; private set; }

        public int Gold { get; private set; }
        public int Level { get; private set; }
        public int Threshold => Mathf.Max(1, Mathf.RoundToInt(goldThresholdCurve.Evaluate(Level)) + Level * thresholdBonusPerLevel);

        // (gold towards the next level, gold needed for it, level)
        public event Action<int, int, int> OnGoldChanged;

        private void Awake()
        {
            if (goldThresholdCurve == null || goldThresholdCurve.length == 0)
                goldThresholdCurve = CurveUtils.EaseIn(0f, 3f, 500f, 20f);
        }

        private void OnEnable()
        {
            if (Current != null && Current != this)
                Debug.LogWarning("TeamLevel: More than one in the scene; the newest one is used.", this);
            Current = this;
        }

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        public void AddGold(int amount)
        {
            Gold += amount;

            int threshold = Threshold;
            while (Gold >= threshold)
            {
                Gold -= threshold;
                Level++;
                GlobalEvents.LevelUp(Level);
                threshold = Threshold;
            }

            OnGoldChanged?.Invoke(Gold, threshold, Level);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (goldThresholdCurve == null || goldThresholdCurve.length == 0)
                goldThresholdCurve = CurveUtils.EaseIn(0f, 3f, 500f, 20f);
        }
#endif
    }
}
