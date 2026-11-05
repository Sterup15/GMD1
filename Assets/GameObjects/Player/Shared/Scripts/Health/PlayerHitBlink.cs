using System.Collections;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Health
{
    public class PlayerHitBlink : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Seconds between each blink toggle")]
        [SerializeField] private float blinkInterval = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float blinkAlpha = 0.2f;

        private ActorEvents _events;
        private Coroutine _blinkCoroutine;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()
        {
            _events.OnHitInvulnerabilityStarted += StartBlink;
            _events.OnHitInvulnerabilityEnded   += StopBlink;
        }

        private void OnDisable()
        {
            _events.OnHitInvulnerabilityStarted -= StartBlink;
            _events.OnHitInvulnerabilityEnded   -= StopBlink;
            StopBlink();
        }

        private void StartBlink()
        {
            if (spriteRenderer == null)
            {
                Debug.LogWarning("PlayerHitBlink: No SpriteRenderer assigned.", this);
                return;
            }

            StopBlink();
            _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        private void StopBlink()
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
            SetAlpha(1f);
        }

        private IEnumerator BlinkRoutine()
        {
            var wait = new WaitForSeconds(blinkInterval);
            bool faded = false;
            while (true)
            {
                faded = !faded;
                SetAlpha(faded ? blinkAlpha : 1f);
                yield return wait;
            }
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer == null) return;
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
