using TMPro;
using UnityEngine;

namespace Actors.Enemy.Scripts.Health
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private float floatSpeed = 1.5f;

        private float _elapsed;
        private Color _color;

        public void Setup(int damage)
        {
            text.text = damage.ToString();
            _color = text.color;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            text.color = new Color(_color.r, _color.g, _color.b, 1f - _elapsed);

            if (_elapsed >= 1f)
                Destroy(gameObject);
        }
    }
}