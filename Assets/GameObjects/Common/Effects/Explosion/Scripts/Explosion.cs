using GameObjects.Common.Audio.Scripts;
using GameObjects.Common.Projectiles.Scripts;
using UnityEngine;

namespace GameObjects.Common.Effects.Explosion.Scripts
{
    // Area damage with an explosion animation. The blast and the clean-up are animation events on the clip
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayers = 1 << 9;
        [Tooltip("Blast radius the sprite shows at scale 1, so the visual can be scaled to the damage radius")]
        [SerializeField] private float spriteRadius = 1.2f;
        [Tooltip("Played once per blast, whether it hits anything or not")]
        [SerializeField] private SoundPool hitSound = new();

        private const int MaxHits = 64;
        private static readonly Collider2D[] Hits = new Collider2D[MaxHits];

        private int _damage;
        private float _radius;

        public void Init(int damage, float radius)
        {
            _damage = damage;
            _radius = radius;
            transform.localScale = Vector3.one * (radius / spriteRadius);
        }

        // Animation event on the frame where the blast is at full size
        public void OnBlast()
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = targetLayers, useTriggers = true };
            int count = Physics2D.OverlapCircle(transform.position, _radius, filter, Hits);

            for (int i = 0; i < count; i++)
            {
                // An enemy can have several colliders, so only damage each GameObject once
                if (HitEarlier(i)) continue;
                if (!Hits[i].TryGetComponent<IDamageable>(out var target)) continue;
                target.TakeDamage(_damage);
            }
            SoundEffects.Play(hitSound);
        }

        private static bool HitEarlier(int index)
        {
            var go = Hits[index].gameObject;
            for (int j = 0; j < index; j++)
                if (Hits[j].gameObject == go) return true;
            return false;
        }

        // Animation event on the last frame
        public void OnExplosionComplete() => Destroy(gameObject);

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _radius > 0f ? _radius : spriteRadius);
        }
#endif
    }
}
