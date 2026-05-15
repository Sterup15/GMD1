using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Music.Scripts
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip normalMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip deathStinger;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void Start() => Play(normalMusic);

        private void OnEnable()
        {
            GlobalEvents.OnBossSpawned  += PlayBossMusic;
            GlobalEvents.OnBossDefeated += PlayNormalMusic;
            GlobalEvents.OnPlayerDied   += PlayDeathStinger;
        }

        private void OnDisable()
        {
            GlobalEvents.OnBossSpawned  -= PlayBossMusic;
            GlobalEvents.OnBossDefeated -= PlayNormalMusic;
            GlobalEvents.OnPlayerDied   -= PlayDeathStinger;
        }

        private void PlayNormalMusic()  => Play(normalMusic);
        private void PlayBossMusic()    => Play(bossMusic);
        private void PlayDeathStinger() => Play(deathStinger);

        private void Play(AudioClip clip)
        {
            if (clip == null || audioSource.clip == clip) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
