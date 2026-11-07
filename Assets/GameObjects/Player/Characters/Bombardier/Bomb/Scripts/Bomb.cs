using System;
using GameObjects.Common.Utils;
using UnityEngine;
using ExplosionEffect = GameObjects.Common.Effects.Explosion.Scripts.Explosion;

namespace GameObjects.Player.Characters.Bombardier.Bomb.Scripts
{
    // Flies in an arc to the spot it was thrown at and explodes on landing, or lies where it was dropped until its fuse runs out.
    // Pooled (PrefabPool): Launch resets the flight, and the Animator is back on its default clip after every reuse
    public class Bomb : MonoBehaviour
    {
        private const string FuseLitState = "FuseLit";

        [Tooltip("Child holding the sprite. It is raised along the arc while the root moves over the ground")]
        [SerializeField] private Transform sprite;
        [SerializeField] private ExplosionEffect explosionPrefab;
        [Tooltip("Ground speed in units per second")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float minFlightTime = 0.4f;
        [Tooltip("Peak height of the arc in world units")]
        [SerializeField] private float arcHeight = 1.2f;
        [Tooltip("Damage radius of the explosion in world units, before the thrower's Area stat")]
        [SerializeField] private float explosionRadius = 1.2f;

        private Animator _animator;
        private Vector2 _start;
        private Vector2 _target;
        private float _duration;
        private float _arc;
        private float _progress;
        private int _damage;
        private float _radius;
        private Action<Vector2, int, float> _onExploded;

        private void Awake() => _animator = sprite.GetComponent<Animator>();

        // areaMultiplier is the thrower's Area stat (1 = the base radius). onExploded reports where it went off
        public void Launch(Vector2 target, int damage, float areaMultiplier, Action<Vector2, int, float> onExploded = null)
        {
            _onExploded = onExploded;
            _radius   = explosionRadius * Mathf.Max(0.1f, areaMultiplier);
            _start    = transform.position;
            _target   = target;
            _damage   = damage;
            _duration = Mathf.Max(minFlightTime, Vector2.Distance(_start, _target) / speed);
            _arc      = arcHeight;
            _progress = 0f;
        }

        // Leaves the bomb where it is and blows it up after fuseTime seconds
        public void Drop(float fuseTime, int damage, float areaMultiplier, Action<Vector2, int, float> onExploded = null)
        {
            Launch(transform.position, damage, areaMultiplier, onExploded);
            _duration = Mathf.Max(0.01f, fuseTime);
            _arc      = 0f;
            if (_animator != null) _animator.Play(FuseLitState);
        }

        private void Update()
        {
            _progress += Time.deltaTime / _duration;
            float t = Mathf.Clamp01(_progress);

            transform.position   = Vector2.Lerp(_start, _target, t);
            sprite.localPosition = new Vector3(0f, _arc * 4f * t * (1f - t), 0f);

            if (_progress >= 1f) Explode();
        }

        private void Explode()
        {
            var explosion = PrefabPool.Spawn(explosionPrefab, _target, Quaternion.identity);
            explosion.Init(_damage, _radius);
            _onExploded?.Invoke(_target, _damage, _radius);
            PooledObject.Release(gameObject);
        }
    }
}
