using GameObjects.Common.Events;
using GameObjects.Common.Settings.Scripts;
using UnityEngine;

namespace GameObjects.Common.Audio.Scripts
{
    public class ActorSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundPool onSpawnPool = new();
        [SerializeField] private SoundPool onShotFiredPool = new();
        [SerializeField] private SoundPool onDeathStartedPool = new();
        [Tooltip("Played on the shared SoundEffects source, so it outlives the actor being destroyed on death")]
        [SerializeField] private SoundPool onDeathPool = new();
        [SerializeField] private SoundPool onHitPool = new();
        [SerializeField] private SoundPool onStepPool = new();
        [SerializeField] private SoundPool onSwingPool = new();
        [SerializeField] private SoundPool onHealedPool = new();

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
            _events.OnHealed        += OnHealed;
        }

        private void OnDisable()
        {
            _events.OnShotFired     -= OnShotFired;
            _events.OnDeathStarted  -= OnDeathStarted;
            _events.OnDeath         -= OnDeath;
            _events.OnHit           -= OnHit;
            _events.OnStep          -= OnStep;
            _events.OnSwing         -= OnSwing;
            _events.OnHealed        -= OnHealed;
        }

        private void OnShotFired()    => Play(onShotFiredPool);
        private void OnDeathStarted() => Play(onDeathStartedPool);
        private void OnDeath()        => SoundEffects.Play(onDeathPool);
        private void OnHit()          => Play(onHitPool);
        private void OnStep()         => Play(onStepPool);
        private void OnSwing()        => Play(onSwingPool);
        private void OnHealed()       => Play(onHealedPool);

        private void Play(SoundPool pool)
        {
            var clip = pool.Pick();
            if (clip == null) return;
            audioSource.PlayOneShot(clip, pool.Volume * GameSettings.EffectsVolume);
        }
    }
}
