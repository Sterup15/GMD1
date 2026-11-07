using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameObjects.Common.Utils
{
    // The one place that changes Time.timeScale. Every screen that stops the game asks for a pause for itself and
    // releases it when it closes; time only runs again once nothing holds a pause, so two screens can't unpause each
    // other. Loading a scene drops every pause, so no exit path has to remember to restore the time scale.
    // Online co-op will decide here what may stop time for everyone
    public static class GamePause
    {
        private static readonly HashSet<object> Holders = new();

        public static bool IsPaused => Holders.Count > 0;

        public static void Pause(object holder)
        {
            if (Holders.Add(holder)) Apply();
        }

        public static void Resume(object holder)
        {
            if (Holders.Remove(holder)) Apply();
        }

        private static void Apply() => Time.timeScale = IsPaused ? 0f : 1f;

        private static void Clear(Scene _, LoadSceneMode __)
        {
            Holders.Clear();
            Apply();
        }

        // Static state survives play mode restarts when domain reload is off, so start clean each time
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Holders.Clear();
            SceneManager.sceneLoaded -= Clear;
            SceneManager.sceneLoaded += Clear;
        }
    }
}
