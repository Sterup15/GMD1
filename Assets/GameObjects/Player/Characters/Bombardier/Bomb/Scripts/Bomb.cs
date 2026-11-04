using UnityEngine;
using ExplosionEffect = GameObjects.Common.Effects.Explosion.Scripts.Explosion;

namespace GameObjects.Player.Characters.Bombardier.Bomb.Scripts
{
    // Flies in an arc to the spot it was thrown at and explodes on landing
    public class Bomb : MonoBehaviour
    {
        [Tooltip("Child holding the sprite. It is raised along the arc while the root moves over the ground")]
        [SerializeField] private Transform sprite;
        [SerializeField] private ExplosionEffect explosionPrefab;
        [Tooltip("Ground speed in units per second")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float minFlightTime = 0.4f;
        [Tooltip("Peak height of the arc in world units")]
        [SerializeField] private float arcHeight = 1.2f;
        [Tooltip("Damage radius of the explosion in world units")]
        [SerializeField] private float explosionRadius = 1.2f;

        private Vector2 _start;
        private Vector2 _target;
        private float _duration;
        private float _progress;
        private int _damage;

        public void Launch(Vector2 target, int damage)
        {
            _start    = transform.position;
            _target   = target;
            _damage   = damage;
            _duration = Mathf.Max(minFlightTime, Vector2.Distance(_start, _target) / speed);
        }

        private void Update()
        {
            _progress += Time.deltaTime / _duration;
            float t = Mathf.Clamp01(_progress);

            transform.position   = Vector2.Lerp(_start, _target, t);
            sprite.localPosition = new Vector3(0f, arcHeight * 4f * t * (1f - t), 0f);

            if (_progress >= 1f) Explode();
        }

        private void Explode()
        {
            var explosion = Instantiate(explosionPrefab, _target, Quaternion.identity);
            explosion.Init(_damage, explosionRadius);
            Destroy(gameObject);
        }
    }
}
