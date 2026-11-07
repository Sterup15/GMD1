using GameObjects.Common.Events;
using GameObjects.Common.Settings.Scripts;
using UnityEngine;

namespace GameObjects.Music.Scripts
{
    // Plays the scene's music: the normal track from the start, and in the game scene the boss, defeat and victory
    // tracks on their GlobalEvents (the main menu only sets the normal track). Every track loops, and the volume
    // follows the Music setting
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
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
