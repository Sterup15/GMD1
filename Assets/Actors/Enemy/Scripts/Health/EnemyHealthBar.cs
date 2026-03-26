using UnityEngine;

namespace Actors.Enemy.Scripts.Health
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer fill;

        private float _fullWidth;

        private void Awake()
        {
            _fullWidth = fill.transform.localScale.x;
        }

        public void SetHealth(int current, int max)
        {
            float t = Mathf.Clamp01((float)current / max);
            float newWidth = _fullWidth * t;
            fill.transform.localScale = new Vector3(newWidth, fill.transform.localScale.y, 1f);

            float offset = (_fullWidth - newWidth) / 2f;
            var pos = fill.transform.localPosition;
            fill.transform.localPosition = new Vector3(-offset, pos.y, pos.z);
        }
    }
}
