using UnityEngine;

namespace GameObjects.Common.Effects.DustPuff.Scripts
{
    public class DustPuff : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Animator state names, one is picked at random")]
        [SerializeField] private string[] clipNames = { "Dust01", "Dust02" };

        private void Start()
        {
            spriteRenderer.flipX = Random.value < 0.5f;
            animator.Play(clipNames[Random.Range(0, clipNames.Length)]);
        }

        // Animation event on the last frame of each dust clip
        public void OnDustComplete() => Destroy(gameObject);

        public static void SpawnBurst(GameObject prefab, Vector2 center, int count, float spreadRadius, Vector2 scaleRange)
        {
            for (int i = 0; i < count; i++)
            {
                var position = center + Random.insideUnitCircle * spreadRadius;
                var puff = Instantiate(prefab, position, Quaternion.identity);
                puff.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            }
        }
    }
}
