using UnityEngine;

namespace Actors.Projectile.Scripts
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void SetDamage(int value) => damage = value;

        public void Launch(Vector2 direction)
        {
            direction = direction.normalized;
            rb.linearVelocity = direction * speed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}