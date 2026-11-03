using System;
using System.Collections.Generic;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Audio.Scripts
{
    // A class rather than a struct so new pools default to full volume instead of 0
    [Serializable]
    public class SoundPool
    {
        [SerializeField] private AudioClip[] clips;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        [Tooltip("Min seconds before the same clip can play again, shared by every actor (0 = no limit)")]
        [SerializeField] private float cooldown;

        // Static so the cooldown limits a clip across all actors, e.g. one hit sound when many enemies are hit at once
        private static readonly Dictionary<AudioClip, float> NextPlayTime = new();

        public float Volume  => volume;
        public bool HasClips => clips is { Length: > 0 };

        public AudioClip Pick()
        {
            if (!HasClips) return null;

            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            if (clip == null || cooldown <= 0f) return clip;

            if (NextPlayTime.TryGetValue(clip, out float next) && Time.time < next) return null;
            NextPlayTime[clip] = Time.time + cooldown;
            return clip;
        }
    }

    public class ActorSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundPool onSpawnPool = new();
        [SerializeField] private SoundPool onShotFiredPool = new();
        [SerializeField] private SoundPool onDeathStartedPool = new();
        [Tooltip("Played on a detached source, so it outlives the actor being destroyed on death")]
        [SerializeField] private SoundPool onDeathPool = new();
        [SerializeField] private SoundPool onHitPool = new();
        [SerializeField] private SoundPool onStepPool = new();
        [SerializeField] private SoundPool onSwingPool = new();

        private ActorEvents _events;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void Start()     => Play(onSpawnPool);

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

        private void OnShotFired()    => Play(onShotFiredPool);
        private void OnDeathStarted() => Play(onDeathStartedPool);
        private void OnDeath()        => PlayDetached(onDeathPool);
        private void OnHit()          => Play(onHitPool);
        private void OnStep()         => Play(onStepPool);
        private void OnSwing()        => Play(onSwingPool);

        private void Play(SoundPool pool)
        {
            var clip = pool.Pick();
            if (clip == null) return;
            audioSource.PlayOneShot(clip, pool.Volume);
        }

        private void PlayDetached(SoundPool pool)
        {
            var clip = pool.Pick();
            if (clip == null) return;

            var go = new GameObject($"{name} DeathSound");
            go.transform.position = transform.position;
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            source.volume       = audioSource.volume;
            source.spatialBlend = audioSource.spatialBlend;
            source.PlayOneShot(clip, pool.Volume);
            Destroy(go, clip.length);
        }
    }
}
