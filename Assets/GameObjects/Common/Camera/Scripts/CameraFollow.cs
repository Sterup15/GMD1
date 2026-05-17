using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Camera.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private bool _frozen;

        private void Start()
        {
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
