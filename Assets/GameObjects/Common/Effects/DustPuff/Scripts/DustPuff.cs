using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Effects.DustPuff.Scripts
{
    public class DustPuff : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Animator state names, one is picked at random")]
        [SerializeField] private string[] clipNames = { "Dust01", "Dust02" };

        // Pooled: OnEnable runs on every reuse (Start only on the first), so each puff restarts its clip here
        private void OnEnable()
        {
            spriteRenderer.flipX = Random.value < 0.5f;
            animator.Play(clipNames[Random.Range(0, clipNames.Length)], 0, 0f);
        }

        // Animation event on the last frame of each dust clip
        public void OnDustComplete() => PooledObject.Release(gameObject);

        public static void SpawnBurst(GameObject prefab, Vector2 center, int count, float spreadRadius, Vector2 scaleRange)
        {
            for (int i = 0; i < count; i++)
            {
                var position = center + Random.insideUnitCircle * spreadRadius;
                var puff = PrefabPool.Spawn(prefab, position, Quaternion.identity);
                puff.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            }
        }
    }
}
