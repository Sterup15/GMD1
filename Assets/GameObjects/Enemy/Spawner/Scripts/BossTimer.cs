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
            if (timerText != null) timerText.gameObject.SetActive(true);
            UpdateText();
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void UpdateText()
        {
            if (timerText == null) return;
            int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
