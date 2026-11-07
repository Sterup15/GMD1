using System;
using System.Collections.Generic;
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
}
