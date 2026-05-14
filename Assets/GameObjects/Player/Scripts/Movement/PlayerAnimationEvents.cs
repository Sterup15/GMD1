using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Player.Scripts.Movement
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        private ActorEvents _events;

        private void Awake() => _events = GetComponentInParent<ActorEvents>();

        public void OnShotFired() => _events.ShotFired();
    }
}
