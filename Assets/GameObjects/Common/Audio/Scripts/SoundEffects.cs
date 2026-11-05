using GameObjects.Common.Settings.Scripts;
using UnityEngine;

namespace GameObjects.Common.Audio.Scripts
{
    // Plays one-shot sounds that belong to short-lived or pooled objects (a projectile or explosion hitting
    // something) on one shared 2D source, so the sound outlives the object without a new GameObject per sound.
    // Game sounds are 2D, so a shared source sounds the same as one on the object
    public static class SoundEffects
    {
        private static AudioSource _source;

        public static void Play(SoundPool pool)
        {
            var clip = pool?.Pick();
            if (clip == null) return;

            if (_source == null)
            {
                var go = new GameObject("SoundEffects");
                Object.DontDestroyOnLoad(go);
                _source = go.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
            }
            _source.PlayOneShot(clip, pool.Volume * GameSettings.EffectsVolume);
        }
    }
}
