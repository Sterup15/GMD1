using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameObjects.Common.Utils
{
    // One random seed per run, so everything that generates the map can rebuild it the same way on every machine.
    // A new seed is picked each time a scene loads, so every run gets a new map. Read it in Start or later: it is
    // cleared when the scene has loaded, after Awake. Online, the host's seed will be sent to the other players
    public static class RunSeed
    {
        private static bool _picked;
        private static int _value;

        public static int Value
        {
            get
            {
                if (_picked) return _value;
                _value = Random.Range(int.MinValue, int.MaxValue);
                _picked = true;
                return _value;
            }
        }

        // A generator of its own for one user of the seed, so one user's draws never shift another's results.
        // The key (e.g. the scatter's name) must be the same on every machine
        public static System.Random CreateRandom(string key) => new(unchecked(Value * 31 + StableHash(key)));

        // string.GetHashCode can differ between runtimes (editor, WebGL), so use FNV-1a
        private static int StableHash(string text)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return (int)hash;
            }
        }

        private static void Clear(Scene _, LoadSceneMode __) => _picked = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _picked = false;
            SceneManager.sceneLoaded -= Clear;
            SceneManager.sceneLoaded += Clear;
        }
    }
}
