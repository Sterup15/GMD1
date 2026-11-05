using GameObjects.Common.Events;
using GameObjects.Common.Settings.Scripts;
using UnityEngine;

namespace GameObjects.Music.Scripts
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip normalMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip defeatMusic;
        [SerializeField] private AudioClip victoryMusic;

        private float _baseVolume;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            _baseVolume = audioSource.volume;
            ApplyVolume();
        }

        private void ApplyVolume() => audioSource.volume = _baseVolume * GameSettings.MusicVolume;

        private void Start() => Play(normalMusic);

        private void OnEnable()
        {
            GameSettings.OnVolumeChanged += ApplyVolume;
            GlobalEvents.OnBossSpawned  += PlayBossMusic;
            GlobalEvents.OnBossDefeated += PlayNormalMusic;
            GlobalEvents.OnPlayerDied   += PlayDefeatMusic;
            GlobalEvents.OnPlayerWon    += PlayVictoryMusic;
        }

        private void OnDisable()
        {
            GameSettings.OnVolumeChanged -= ApplyVolume;
            GlobalEvents.OnBossSpawned  -= PlayBossMusic;
            GlobalEvents.OnBossDefeated -= PlayNormalMusic;
            GlobalEvents.OnPlayerDied   -= PlayDefeatMusic;
            GlobalEvents.OnPlayerWon    -= PlayVictoryMusic;
        }

        private void PlayNormalMusic()  => Play(normalMusic);
        private void PlayBossMusic()    => Play(bossMusic);
        private void PlayDefeatMusic()  => Play(defeatMusic);
        private void PlayVictoryMusic() => Play(victoryMusic);

        private void Play(AudioClip clip)
        {
            if (clip == null || audioSource.clip == clip) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
