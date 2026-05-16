using System;
using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.Audio.Scripts
{
    [Serializable]
    public struct SoundPool
    {
        [SerializeField] private AudioClip[] clips;

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
            _events.OnShotFired     += OnShotFired;
            _events.OnDeathStarted  += OnDeathStarted;
            _events.OnStep          += OnStep;
            _events.OnSwing         += OnSwing;
        }

        private void OnDisable()
        {
            _events.OnShotFired     -= OnShotFired;
            _events.OnDeathStarted  -= OnDeathStarted;
            _events.OnStep          -= OnStep;
            _events.OnSwing         -= OnSwing;
        }

        private void OnShotFired()    => Play(onShotFiredPool.Pick());
        private void OnDeathStarted() => Play(onDeathStartedPool.Pick());
        private void OnStep()         => Play(onStepPool.Pick());
        private void OnSwing()        => Play(onSwingPool.Pick());

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip);
        }
    }
}
