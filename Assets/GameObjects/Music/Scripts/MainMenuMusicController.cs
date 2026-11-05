using GameObjects.Common.Settings.Scripts;
using UnityEngine;

namespace GameObjects.Music.Scripts
{
    public class MainMenuMusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip menuMusic;

        private float _baseVolume;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            _baseVolume = audioSource.volume;
            ApplyVolume();
        }

        private void ApplyVolume() => audioSource.volume = _baseVolume * GameSettings.MusicVolume;

        private void OnEnable()  => GameSettings.OnVolumeChanged += ApplyVolume;
        private void OnDisable() => GameSettings.OnVolumeChanged -= ApplyVolume;

        private void Start()
        {
            if (menuMusic == null) return;
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
