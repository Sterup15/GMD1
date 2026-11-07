using GameObjects.Common.Utils;
using TMPro;
using UnityEngine;

namespace GameObjects.Common.DamageNumber.Scripts
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private float floatSpeed = 1.5f;

        private float _elapsed;
        private Color _color;

        private void Awake() => _color = text.color;

        // Pooled: every number starts fresh here, since a reused one still has the last number's fade
        public void Setup(int damage)
        {
            text.SetText("{0}", damage);
            text.color = _color;
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            text.color = new Color(_color.r, _color.g, _color.b, 1f - _elapsed);

            if (_elapsed >= 1f)
                PooledObject.Release(gameObject);
        }
    }
}