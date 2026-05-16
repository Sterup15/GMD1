using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.UI.VictoryScene.Scripts
{
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        private void Start()     => panel.SetActive(false);
        private void OnEnable()  => GlobalEvents.OnPlayerWon += Show;
        private void OnDisable() => GlobalEvents.OnPlayerWon -= Show;

        private void Show()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
