using GameObjects.Common.Events;
using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.EndScreen.Scripts
{
    // The victory or defeat screen: shows its panel and pauses when the run ends that way
    public class EndScreen : MenuScreen
    {
        [SerializeField] private EndScreenType showOn;
        [Tooltip("In on-screen order")]
        [SerializeField] private Button[] buttons;

        protected override bool ClosesOnPause => false;

        private void Awake()
        {
            Navigator = new MenuNavigator(buttons);
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

        private void Show()
        {
            ShowPanel();
            GamePause.Pause(this);
            Navigator.Select(0, 0);
        }
    }
}
