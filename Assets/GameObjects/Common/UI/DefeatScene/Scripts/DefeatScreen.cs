using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.UI.Scripts
{
    public class DefeatScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        private void Start()          => panel.SetActive(false);
        private void OnEnable()       => GlobalEvents.OnPlayerDied += Show;
        private void OnDisable()      => GlobalEvents.OnPlayerDied -= Show;

        private void Show()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
