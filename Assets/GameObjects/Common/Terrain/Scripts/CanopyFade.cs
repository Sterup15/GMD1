using UnityEngine;

namespace GameObjects.Common.Terrain.Scripts
{
    // Makes the tree see-through while the player is behind its canopy, so the player never loses sight of themselves mid-barrage.
    // The trigger is on the Terrain layer, which only touches the player's Feet collider (a child, so the tag is on its Rigidbody)
    [RequireComponent(typeof(SpriteRenderer))]
    public class CanopyFade : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.45f;

        private SpriteRenderer _renderer;

        private void Awake() => _renderer = GetComponent<SpriteRenderer>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other)) SetAlpha(fadedAlpha);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayer(other)) SetAlpha(1f);
        }

        private static bool IsPlayer(Collider2D other) =>
            other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player");

        private void SetAlpha(float alpha)
        {
            var color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }
}
