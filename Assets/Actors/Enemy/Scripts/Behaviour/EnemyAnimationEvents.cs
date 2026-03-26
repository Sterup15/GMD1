using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyAnimationEvents : MonoBehaviour
    {
        private EnemyMovementState movementState;

        private void Awake()
        {
            movementState = GetComponentInParent<EnemyMovementState>();
        }

        public void OnHitComplete()
        {
            movementState.OnHitComplete();
        }
    }
}