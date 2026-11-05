using System;
using UnityEngine;

namespace GameObjects.Common.Settings.Scripts
{
    // Player settings that last between sessions (PlayerPrefs). Static on purpose: they must survive scene loads.
    // Fullscreen isn't stored here, because Unity already remembers the window mode on desktop and browsers decide it on the web
    public static class GameSettings
    {
        private const string MasterKey  = "settings.masterVolume";
        private const string MusicKey   = "settings.musicVolume";
        private const string EffectsKey = "settings.effectsVolume";

        public static float MasterVolume  { get; private set; } = 1f;
        public static float MusicVolume   { get; private set; } = 1f;
        public static float EffectsVolume { get; private set; } = 1f;

        // Raised after any volume changes, so audio sources can reapply their level
        public static event Action OnVolumeChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            MasterVolume  = PlayerPrefs.GetFloat(MasterKey, 1f);
            MusicVolume   = PlayerPrefs.GetFloat(MusicKey, 1f);
            EffectsVolume = PlayerPrefs.GetFloat(EffectsKey, 1f);
            AudioListener.volume = MasterVolume;
        }

        public static float Get(SettingType setting) => setting switch
        {
            SettingType.MasterVolume  => MasterVolume,
            SettingType.MusicVolume   => MusicVolume,
            SettingType.EffectsVolume => EffectsVolume,
            SettingType.Fullscreen    => Screen.fullScreen ? 1f : 0f,
            _                         => 0f
        };

        public static void Set(SettingType setting, float value)
        {
            value = Mathf.Clamp01(value);
            switch (setting)
            {
                case SettingType.MasterVolume:
                    MasterVolume = value;
                    AudioListener.volume = value;
                    PlayerPrefs.SetFloat(MasterKey, value);
                    break;
                case SettingType.MusicVolume:
                    MusicVolume = value;
                    PlayerPrefs.SetFloat(MusicKey, value);
                    break;
                case SettingType.EffectsVolume:
                    EffectsVolume = value;
                    PlayerPrefs.SetFloat(EffectsKey, value);
                    break;
                case SettingType.Fullscreen:
                    Screen.fullScreen = value > 0.5f;
                    return;
            }

            PlayerPrefs.Save();
            OnVolumeChanged?.Invoke();
        }
    }
}
