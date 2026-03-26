using UnityEngine;

namespace Actors.Player.Scripts.Movement
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        private PlayerMovementState movementState;

        private void Awake()
        {
            movementState = GetComponentInParent<PlayerMovementState>();
        }

        public void OnShotFired()
        {
            movementState.OnShotFired();
        }
    }
}