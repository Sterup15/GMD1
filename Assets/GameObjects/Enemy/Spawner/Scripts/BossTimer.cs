using System;
using TMPro;
using UnityEngine;

namespace GameObjects.Enemy.Common.Spawner
{
    public class BossTimer : MonoBehaviour
    {
        [SerializeField] private float duration = 60f;
        [SerializeField] private TextMeshProUGUI timerText;

        public event Action OnTimerComplete;

        private float _timeRemaining;
        private bool _isRunning;
        private int _shownSeconds = -1;

        private void Start()
        {
            ResetAndStart();
        }

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isRunning = false;
                if (timerText != null) timerText.gameObject.SetActive(false);
                OnTimerComplete?.Invoke();
                return;
            }

            UpdateText();
        }

        public void ResetAndStart()
        {
            _timeRemaining = duration;
            _isRunning = true;
            _shownSeconds = -1;
            if (timerText != null) timerText.gameObject.SetActive(true);
            UpdateText();
        }

        // Only rebuilds the text when the shown second changes, instead of a new string every frame
        private void UpdateText()
        {
            if (timerText == null) return;
            int totalSeconds = Mathf.FloorToInt(_timeRemaining);
            if (totalSeconds == _shownSeconds) return;
            _shownSeconds = totalSeconds;
            timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
