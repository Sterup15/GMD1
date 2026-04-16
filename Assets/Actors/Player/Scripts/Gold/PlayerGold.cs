using System;
using UnityEngine;

namespace Actors.Player.Scripts.Gold
{
    public class PlayerGold : MonoBehaviour
    {
        [Tooltip("X = player level, Y = gold needed to reach the next level")]
        [SerializeField] private AnimationCurve goldThresholdCurve = AnimationCurve.Linear(0f, 100f, 20f, 500f);

        public static event Action<int, int> OnGoldChanged; // (current, threshold)
        public static event Action OnLevelUp;

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
                OnLevelUp?.Invoke();
                threshold = GetThreshold();
            }

            OnGoldChanged?.Invoke(CurrentGold, threshold);
        }

        private int GetThreshold() => Mathf.Max(1, Mathf.RoundToInt(goldThresholdCurve.Evaluate(Level)));
    }
}
