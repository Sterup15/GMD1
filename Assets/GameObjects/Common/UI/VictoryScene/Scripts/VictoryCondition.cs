using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.UI.VictoryScene.Scripts
{
    public class VictoryCondition : MonoBehaviour
    {
        [SerializeField] private int winLevel = 10;
        public int WinLevel => winLevel;

        private void OnEnable()  => GlobalEvents.OnLevelUp += CheckVictory;
        private void OnDisable() => GlobalEvents.OnLevelUp -= CheckVictory;

        private void CheckVictory(int level)
        {
            if (level >= winLevel)
                GlobalEvents.PlayerWon();
        }
    }
}
