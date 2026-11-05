using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Navigation.Scripts
{
    // Keyboard and gamepad selection shared by every menu, for buttons laid out in rows. With one row any direction
    // steps through it; with several, left/right moves within a row and up/down between rows, coming back to the
    // button last used in that row. It wraps around and skips buttons that are disabled or hidden. It also follows
    // the EventSystem's selection, so keys carry on from a button a click selected.
    // The buttons need Navigation set to None, or the UI input module moves the selection a second time
    public class MenuNavigator
    {
        private readonly Button[][] _rows;
        private readonly int[] _columns;
        private int _blockedFrame = -1;

        public int Row { get; private set; }
        public int Column => _columns[Row];
        public Button Current => _rows[Row][Column];

        // The key press that opened a menu, or closed a screen on top of it, must not also act on it
        public bool InputBlocked => Time.frameCount == _blockedFrame;

        // Raised with (row, column) whenever the selection moves, by a key, a click or Select
        public event Action<int, int> OnSelectionChanged;

        public MenuNavigator(params Button[][] rows)
        {
            _rows = rows;
            _columns = new int[rows.Length];
        }

        public void BlockInputThisFrame() => _blockedFrame = Time.frameCount;

        // Call once per frame while the menu is open and has the input
        public void Navigate()
        {
            SyncWithMouseSelection();

            int horizontal = (GameInput.NavigateRight ? 1 : 0) - (GameInput.NavigateLeft ? 1 : 0);
            int vertical   = (GameInput.NavigateDown  ? 1 : 0) - (GameInput.NavigateUp   ? 1 : 0);

            if (_rows.Length == 1)
            {
                int step = horizontal != 0 ? horizontal : vertical;
                if (step != 0) TrySelect(0, Column + step, step);
            }
            else if (vertical != 0)   SelectRow(Row + vertical, vertical);
            else if (horizontal != 0) TrySelect(Row, Column + horizontal, horizontal);
        }

        public void Submit() => Current.onClick.Invoke();

        public void Select(int row, int column) => TrySelect(row, column, 1);

        public void Select(Button button)
        {
            for (int r = 0; r < _rows.Length; r++)
            {
                int c = Array.IndexOf(_rows[r], button);
                if (c >= 0) { TrySelect(r, c, 1); return; }
            }
        }

        // Rows without anything selectable (e.g. only disabled buttons) are skipped
        private void SelectRow(int row, int step)
        {
            for (int tries = 0; tries < _rows.Length; tries++)
            {
                row = Wrap(row, _rows.Length);
                if (TrySelect(row, _columns[row], step)) return;
                row += step;
            }
        }

        private bool TrySelect(int row, int column, int step)
        {
            var buttons = _rows[row];
            for (int tries = 0; tries < buttons.Length; tries++)
            {
                column = Wrap(column, buttons.Length);
                if (IsSelectable(buttons[column]))
                {
                    Row = row;
                    _columns[row] = column;
                    buttons[column].Select();
                    OnSelectionChanged?.Invoke(row, column);
                    return true;
                }
                column += step;
            }
            return false;
        }

        private void SyncWithMouseSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null || selected == Current.gameObject) return;

            for (int r = 0; r < _rows.Length; r++)
            for (int c = 0; c < _rows[r].Length; c++)
            {
                if (_rows[r][c] == null || _rows[r][c].gameObject != selected) continue;
                Row = r;
                _columns[r] = c;
                OnSelectionChanged?.Invoke(r, c);
                return;
            }
        }

        private static bool IsSelectable(Button button) =>
            button != null && button.interactable && button.gameObject.activeInHierarchy;

        private static int Wrap(int index, int count) => (index % count + count) % count;
    }
}
