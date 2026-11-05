using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Camera.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private bool _frozen;

        private void Start()
        {
            // The player is spawned at runtime by PlayerSpawner, so it can't be assigned in the scene
            if (target == null)
            {
                target = Players.Local;
                if (target == null) Debug.LogWarning("CameraFollow: No local player registered.");
            }

            GlobalEvents.OnBossSpawned  += Freeze;
            GlobalEvents.OnBossDefeated += Unfreeze;
        }

        private void OnDestroy()
        {
            GlobalEvents.OnBossSpawned  -= Freeze;
            GlobalEvents.OnBossDefeated -= Unfreeze;
        }

        private void LateUpdate()
        {
            if (_frozen || target == null) return;

            Vector3 pos = target.position;
            pos.z = transform.position.z;
            transform.position = pos;
        }

        private void Freeze()   => _frozen = true;
        private void Unfreeze() => _frozen = false;
    }
}
