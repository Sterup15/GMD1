using UnityEngine;

namespace Actors.Enemy.Scripts.Behaviour
{
    public class EnemyMeleeAnimationEvents : MonoBehaviour
    {
        private EnemyMeleeMovementState movementState;
        
        private void Awake()
        {
            movementState = GetComponentInParent<EnemyMeleeMovementState>();
        }
        
        public void OnAttackComplete()
        {
            movementState.OnAttackComplete();
        }
    }
}