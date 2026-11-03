using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts.Death
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
    }
}
