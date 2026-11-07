using System;
using UnityEngine;

namespace GameObjects.Common.UI.Navigation.Scripts
{
    // What every full-screen menu shares: a panel that opens and closes (with a callback back to whoever opened it),
    // and the keyboard/gamepad loop: Pause closes, the navigator moves the selection, Interact presses the selected
    // button. A screen creates its Navigator, opens with ShowPanel, and adds its own extras through the hooks
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField] protected GameObject panel;

        protected MenuNavigator Navigator;
        private Action _onClosed;

        public bool IsOpen => panel.activeSelf;

        // Screens that must be answered (upgrade cards, victory, defeat) don't close on Pause
        protected virtual bool ClosesOnPause => true;

        // True while a screen opened from this one has the input
        protected virtual bool IsCovered => false;

        // Shows the panel. The key press that opened it must not also act on it (a screen that builds its Navigator
        // after showing, because its rows need to be active, blocks the input itself)
        protected void ShowPanel(Action onClosed = null)
        {
            _onClosed = onClosed;
            panel.SetActive(true);
            Navigator?.BlockInputThisFrame();
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            OnClosed();
            _onClosed?.Invoke();
        }

        protected virtual void OnClosed() { }

        protected virtual void Update()
        {
            if (!IsOpen || Navigator == null || Navigator.InputBlocked || IsCovered) return;

            if (ClosesOnPause && GameInput.PausePressed)
            {
                Close();
                return;
            }

            Navigator.Navigate();
            HandleExtraInput();
            if (GameInput.InteractPressed) Interact();
        }

        // Runs after navigation on every frame the screen has the input (e.g. left/right changing a value)
        protected virtual void HandleExtraInput() { }

        protected virtual void Interact() => Navigator.Submit();

        // Copies an inactive row template next to it, rowSpacing further down for every index
        protected static T CreateRow<T>(T template, int index, float rowSpacing) where T : Component
        {
            var row = Instantiate(template, template.transform.parent);
            row.gameObject.SetActive(true);
            var start = ((RectTransform)template.transform).anchoredPosition;
            ((RectTransform)row.transform).anchoredPosition = start + Vector2.down * (rowSpacing * index);
            return row;
        }
    }
}
