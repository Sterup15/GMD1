using UnityEngine;

namespace GameObjects.Common.Projectiles.Scripts
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D _rb;
        private int _bouncesRemaining;
        private int _penetrationsRemaining;
        private float _lastBounceTime = float.MinValue;
        private static int _cameraWallsLayer;
        private const float BounceCooldown = 0.1f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _cameraWallsLayer = LayerMask.NameToLayer("CameraWalls");
        }

        public void SetDamage(int value)        => damage = value;
        public void SetBounces(int count)       => _bouncesRemaining = count;
        public void SetPenetration(int count)   => _penetrationsRemaining = count;

        public void Launch(Vector2 direction)
        {
            direction = direction.normalized;
            _rb.linearVelocity = direction * speed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == _cameraWallsLayer)
            {
                if (Time.time - _lastBounceTime < BounceCooldown) return;
                _lastBounceTime = Time.time;

                if (_bouncesRemaining <= 0)
                {
                    Destroy(gameObject);
                    return;
                }

                Vector2 vel = _rb.linearVelocity;
                // Wide collider = horizontal wall (top/bottom), reflect Y; tall = vertical (left/right), reflect X
                if (other.bounds.size.x > other.bounds.size.y)
                    vel.y = -vel.y;
                else
                    vel.x = -vel.x;

                _rb.linearVelocity = vel;
                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                _bouncesRemaining--;
                return;
            }

            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
                if (_penetrationsRemaining <= 0)
                    Destroy(gameObject);
                else
                    _penetrationsRemaining--;
            }
        }
    }
}