using UnityEngine;
using UnityEngine.EventSystems;

namespace GameObjects.Common.UI.Pointer.Scripts
{
    // A pointing hand that sits on whichever button is selected, in every menu (main menu, upgrade cards,
    // pause, settings, victory, defeat), so keyboard and gamepad players can see their selection.
    // Spawned once from Resources and kept across scenes, so no menu needs to know about it.
    public class SelectionPointer : MonoBehaviour
    {
        private const string PrefabPath = "UI/SelectionPointer";

        [SerializeField] private RectTransform pointer;
        [Tooltip("Where the fingertip lands on the selected button, 0-1 across its width and height")]
        [SerializeField] private Vector2 pointOnTarget = new(0.85f, 0.3f);
        [Tooltip("How far the hand bobs, in screen pixels at 1080p")]
        [SerializeField] private float bobDistance = 4f;
        [SerializeField] private float bobSpeed = 6f;

        private readonly Vector3[] _corners = new Vector3[4];
        private GameObject _target;
        private RectTransform _targetRect;
        private Canvas _targetCanvas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            var prefab = Resources.Load<SelectionPointer>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"SelectionPointer: No prefab at Resources/{PrefabPath}.");
                return;
            }
            DontDestroyOnLoad(Instantiate(prefab));
        }

        private void LateUpdate()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != _target) Retarget(selected);

            bool visible = _targetRect != null && _target.activeInHierarchy;
            if (pointer.gameObject.activeSelf != visible) pointer.gameObject.SetActive(visible);
            if (!visible) return;

            _targetRect.GetWorldCorners(_corners);
            Vector3 world = new(
                Mathf.Lerp(_corners[0].x, _corners[2].x, pointOnTarget.x),
                Mathf.Lerp(_corners[0].y, _corners[2].y, pointOnTarget.y),
                _corners[0].z);

            // Overlay canvases are already in screen pixels; camera canvases need projecting
            Vector2 screen = _targetCanvas == null || _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? world
                : RectTransformUtility.WorldToScreenPoint(_targetCanvas.worldCamera, world);

            // Unscaled time, because most menus are open while the game is paused
            float bob = Mathf.Sin(Time.unscaledTime * bobSpeed) * bobDistance * Screen.height / 1080f;
            pointer.position = screen + new Vector2(0f, bob);
        }

        private void Retarget(GameObject selected)
        {
            _target       = selected;
            _targetRect   = selected != null ? selected.transform as RectTransform : null;
            _targetCanvas = _targetRect != null ? _targetRect.GetComponentInParent<Canvas>() : null;
        }
    }
}
