using System;
using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Player.Scripts.Gold
{
    public class PlayerGold : MonoBehaviour
    {
        [Tooltip("X = player level, Y = gold needed to reach the next level")]
        [SerializeField] private AnimationCurve goldThresholdCurve;

        public static event Action<int, int> OnGoldChanged; // (current, threshold)

        [SerializeField] private int thresholdBonusPerLevel = 3;

        private void Awake()
        {
            if (goldThresholdCurve == null || goldThresholdCurve.length == 0)
                goldThresholdCurve = CurveUtils.EaseIn(0f, 3f, 500f, 20f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (goldThresholdCurve == null || goldThresholdCurve.length == 0)
                goldThresholdCurve = CurveUtils.EaseIn(0f, 3f, 500f, 20f);
        }
#endif

        public int CurrentGold { get; private set; }
        public int Level { get; private set; }

        public void AddGold(int amount)
        {
            CurrentGold += amount;

            int threshold = GetThreshold();
            while (CurrentGold >= threshold)
            {
                CurrentGold -= threshold;
                Level++;
                GlobalEvents.LevelUp(Level);
                threshold = GetThreshold();
            }

            OnGoldChanged?.Invoke(CurrentGold, threshold);
        }

        private int GetThreshold() => Mathf.Max(1, Mathf.RoundToInt(goldThresholdCurve.Evaluate(Level)) + Level * thresholdBonusPerLevel);
    }
}
