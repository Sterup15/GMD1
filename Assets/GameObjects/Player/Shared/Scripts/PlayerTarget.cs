using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts
{
    // Registers this character with Players while it exists, so cameras, HUD, spawners and enemies find it there
    // instead of by tag. It registers in OnEnable, which runs inside PlayerSpawner's Instantiate, so everything
    // that looks it up in Start finds it. On death it is marked out, so enemies stop going after it
    public class PlayerTarget : MonoBehaviour
    {
        [Tooltip("Controlled on this device. Every player is local in solo; online, other players' copies won't be")]
        [SerializeField] private bool isLocal = true;

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()
        {
            Players.Add(transform, isLocal);
            if (_events != null) _events.OnDeath += MarkOut;
        }

        private void OnDisable()
        {
            if (_events != null) _events.OnDeath -= MarkOut;
            Players.Remove(transform);
        }

        private void MarkOut() => Players.SetOut(transform, true);
    }
}
