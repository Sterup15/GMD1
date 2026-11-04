using GameObjects.Common.Effects.DustPuff.Scripts;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Player.Scripts.Dash
{
    public class DashDust : MonoBehaviour
    {
        [SerializeField] private GameObject dustPuffPrefab;
        [SerializeField, Min(1)] private int puffCount = 3;
        [Tooltip("Max distance from the take-off point a puff can spawn")]
        [SerializeField] private float spreadRadius = 0.3f;
        [Tooltip("Random uniform scale per puff (min, max)")]
        [SerializeField] private Vector2 scaleRange = new(1f, 1.4f);

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();
        private void OnEnable()  => _events.OnDashStarted += SpawnDust;
        private void OnDisable() => _events.OnDashStarted -= SpawnDust;

        private void SpawnDust(Vector2 _)
        {
            if (dustPuffPrefab == null)
            {
                Debug.LogWarning("DashDust: No dust puff prefab assigned.", this);
                return;
            }

            DustPuff.SpawnBurst(dustPuffPrefab, transform.position, puffCount, spreadRadius, scaleRange);
        }
    }
}
