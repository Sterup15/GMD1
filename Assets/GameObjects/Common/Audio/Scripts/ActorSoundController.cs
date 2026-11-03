using System;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Audio.Scripts
{
    [Serializable]
    public struct SoundPool
    {
        [SerializeField] private AudioClip[] clips;

        public bool HasClips => clips is { Length: > 0 };

        public AudioClip Pick()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }

    public class ActorSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundPool onSpawnPool;
        [SerializeField] private SoundPool onShotFiredPool;
        [SerializeField] private SoundPool onDeathStartedPool;
        [Tooltip("Played on a detached source, so it outlives the actor being destroyed on death")]
        [SerializeField] private SoundPool onDeathPool;
        [SerializeField] private SoundPool onHitPool;
        [SerializeField] private SoundPool onStepPool;
        [SerializeField] private SoundPool onSwingPool;

        private ActorEvents _events;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void Start()     => Play(onSpawnPool.Pick());

        private void OnEnable()
        {
            // EnemyHealth waits for a death animation whenever OnDeathStarted has listeners,
            // so only listen when there is a sound to play
            if (onDeathStartedPool.HasClips)
                _events.OnDeathStarted += OnDeathStarted;

            _events.OnShotFired     += OnShotFired;
            _events.OnDeath         += OnDeath;
            _events.OnHit           += OnHit;
            _events.OnStep          += OnStep;
            _events.OnSwing         += OnSwing;
        }

        private void OnDisable()
        {
            _events.OnShotFired     -= OnShotFired;
            _events.OnDeathStarted  -= OnDeathStarted;
            _events.OnDeath         -= OnDeath;
            _events.OnHit           -= OnHit;
            _events.OnStep          -= OnStep;
            _events.OnSwing         -= OnSwing;
        }

        private void OnShotFired()    => Play(onShotFiredPool.Pick());
        private void OnDeathStarted() => Play(onDeathStartedPool.Pick());
        private void OnDeath()        => PlayDetached(onDeathPool.Pick());
        private void OnHit()          => Play(onHitPool.Pick());
        private void OnStep()         => Play(onStepPool.Pick());
        private void OnSwing()        => Play(onSwingPool.Pick());

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip);
        }

        private void PlayDetached(AudioClip clip)
        {
            if (clip == null) return;

            var go = new GameObject($"{name} DeathSound");
            go.transform.position = transform.position;
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            source.volume       = audioSource.volume;
            source.spatialBlend = audioSource.spatialBlend;
            source.PlayOneShot(clip);
            Destroy(go, clip.length);
        }
    }
}
