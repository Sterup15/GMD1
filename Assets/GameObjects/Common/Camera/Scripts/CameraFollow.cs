using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Camera.Scripts
{
    // Follows the local player, and during a boss fight locks onto the arena and zooms so all of it fits this screen
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private UnityEngine.Camera _camera;
        private float _normalSize;
        private bool _locked;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera != null) _normalSize = _camera.orthographicSize;
        }

        private void Start()
        {
            // The player is spawned at runtime by PlayerSpawner, so it can't be assigned in the scene
            if (target == null)
            {
                target = Players.Local;
                if (target == null) Debug.LogWarning("CameraFollow: No local player registered.");
            }

            GlobalEvents.OnBossArenaOpened += LockOnArena;
            GlobalEvents.OnBossArenaClosed += Unlock;
        }

        private void OnDestroy()
        {
            GlobalEvents.OnBossArenaOpened -= LockOnArena;
            GlobalEvents.OnBossArenaClosed -= Unlock;
        }

        private void LateUpdate()
        {
            if (_locked || target == null) return;

            Vector3 pos = target.position;
            pos.z = transform.position.z;
            transform.position = pos;
        }

        // Zooms out only as far as this screen's aspect needs, so a PC sees about what it saw before the fight
        private void LockOnArena(Vector2 center, Vector2 size)
        {
            _locked = true;
            transform.position = new Vector3(center.x, center.y, transform.position.z);
            if (_camera != null)
                _camera.orthographicSize = Mathf.Max(size.y / 2f, size.x / 2f / _camera.aspect);
        }

        private void Unlock()
        {
            _locked = false;
            if (_camera != null) _camera.orthographicSize = _normalSize;
        }
    }
}
