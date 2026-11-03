using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts.Death
{
    public class DeathDust : MonoBehaviour
    {
        [SerializeField] private GameObject dustPuffPrefab;
        [SerializeField, Min(1)] private int puffCount = 3;
        [Tooltip("Max distance from the enemy's centre a puff can spawn")]
        [SerializeField] private float spreadRadius = 0.25f;
        [Tooltip("Random uniform scale per puff (min, max)")]
        [SerializeField] private Vector2 scaleRange = new(1.2f, 1.6f);

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();
        private void OnEnable()  => _events.OnDeath += SpawnDust;
        private void OnDisable() => _events.OnDeath -= SpawnDust;

        private void SpawnDust()
        {
            if (dustPuffPrefab == null)
            {
                Debug.LogWarning("DeathDust: No dust puff prefab assigned.", this);
                return;
            }

            for (int i = 0; i < puffCount; i++)
            {
                var position = (Vector2)transform.position + Random.insideUnitCircle * spreadRadius;
                var puff = Instantiate(dustPuffPrefab, position, Quaternion.identity);
                puff.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            }
        }
    }
}
