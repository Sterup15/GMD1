using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class EnemyRangedAnimationEvents : MonoBehaviour
    {
        private EnemyRangedMovementState movementState;

        private void Awake()
        {
            movementState = GetComponentInParent<EnemyRangedMovementState>();
        }

        public void OnShotFired()
        {
            movementState.OnShotFired();
        }
    }
}