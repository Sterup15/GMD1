using System;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private EnemyFollow enemyFollow;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string hitAnim = "Hit";
        public static Action<EnemyMoveStateEnum> OnEnemyMoveStateChanged;

        private void Awake()
        {
            enemyFollow = GetComponent<EnemyFollow>();
        }

        private void Update()
        {
            if (CurrentMoveState == EnemyMoveStateEnum.Hit) return;

            SetMoveState(enemyFollow.IsMoving ? EnemyMoveStateEnum.Move : EnemyMoveStateEnum.Idle);

            if (enemyFollow.MoveDirection.x != 0f)
                spriteRenderer.flipX = enemyFollow.MoveDirection.x < 0f;
        }

        public void SetMoveState(EnemyMoveStateEnum newMoveState)
        {
            if (CurrentMoveState == newMoveState) return;

            switch (newMoveState)
            {
                case EnemyMoveStateEnum.Idle:
                    HandleIdle();
                    break;

                case EnemyMoveStateEnum.Move:
                    HandleMove();
                    break;

                case EnemyMoveStateEnum.Hit:
                    HandleHit();
                    break;

                default:
                    Debug.LogError($"Invalid move state: {newMoveState}");
                    break;
            }

            OnEnemyMoveStateChanged?.Invoke(newMoveState);
            CurrentMoveState = newMoveState;
        }

        private void HandleIdle()
        {
            animator.Play(idleAnim);
        }

        private void HandleMove()
        {
            animator.Play(moveAnim);
        }

        private void HandleHit()
        {
            animator.Play(hitAnim);
        }

        // Call this from an Animation Event at the end of the Hit clip
        public void OnHitComplete()
        {
            CurrentMoveState = EnemyMoveStateEnum.Idle; // allow Update to reassign
        }
    }
}