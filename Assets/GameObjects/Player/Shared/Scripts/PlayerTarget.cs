using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts
{
    // Registers this character with Players while it exists, so cameras, HUD, spawners and enemies find it there
    // instead of by tag. It registers in OnEnable, which runs inside PlayerSpawner's Instantiate, so everything
    // that looks it up in Start finds it
    public class PlayerTarget : MonoBehaviour
    {
        [Tooltip("Controlled on this device. Every player is local in solo; online, other players' copies won't be")]
        [SerializeField] private bool isLocal = true;

        private void OnEnable()  => Players.Add(transform, isLocal);
        private void OnDisable() => Players.Remove(transform);
    }
}
