using System;
using UnityEngine;

namespace GameObjects.Enemy.TogreBoss.Scripts.Behaviour
{
    public class BossHitbox : MonoBehaviour
    {
        public event Action<Collider2D> OnHit;
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.enabled = false;
        }

        public void Open() => _collider.enabled = true;
        public void Close() => _collider.enabled = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnHit?.Invoke(other);
        }
    }
}
