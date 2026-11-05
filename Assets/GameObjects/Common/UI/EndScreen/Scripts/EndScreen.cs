using GameObjects.Common.Events;
using GameObjects.Common.UI.Navigation.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.EndScreen.Scripts
{
    // The victory or defeat screen: shows its panel and pauses when the run ends that way
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private EndScreenType showOn;
        [SerializeField] private GameObject panel;
        [Tooltip("In on-screen order")]
        [SerializeField] private Button[] buttons;

        private MenuNavigator _navigator;

        private void Awake()
        {
            _navigator = new MenuNavigator(buttons);
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (showOn == EndScreenType.Victory) GlobalEvents.OnPlayerWon  += Show;
            else                                 GlobalEvents.OnPlayerDied += Show;
        }

        private void OnDisable()
        {
            GlobalEvents.OnPlayerWon  -= Show;
            GlobalEvents.OnPlayerDied -= Show;
        }

        private void Update()
        {
            if (!panel.activeSelf || _navigator.InputBlocked) return;

            _navigator.Navigate();
            if (GameInput.InteractPressed) _navigator.Submit();
        }

        private void Show()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
            _navigator.BlockInputThisFrame();
            _navigator.Select(0, 0);
        }
    }
}
