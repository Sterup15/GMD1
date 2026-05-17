using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.UI.VictoryScene.Scripts
{
    public class VictoryCondition : MonoBehaviour
    {
        private void OnEnable()  => GlobalEvents.OnBossDefeated += GlobalEvents.PlayerWon;
        private void OnDisable() => GlobalEvents.OnBossDefeated -= GlobalEvents.PlayerWon;
    }
}
