using System;
using Actors.Enemy.Scripts.Behaviour;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyMeleeMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private EnemyMelee mover;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string attackAnim = "Attack";
        public static Action<EnemyMoveStateEnum> OnEnemyMoveStateChanged;

        private void Awake()
        {
            mover = GetComponent<EnemyMelee>();
        }

        private void Update()
        {
            if (CurrentMoveState == EnemyMoveStateEnum.Attack) return;

            SetMoveState(mover.IsMoving ? EnemyMoveStateEnum.Move : EnemyMoveStateEnum.Idle);

            if (mover.MoveDirection.x != 0f)
                spriteRenderer.flipX = mover.MoveDirection.x < 0f;
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

                case EnemyMoveStateEnum.Attack:
                    HandleAttack();
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
            animator.speed = 1f;
            animator.Play(idleAnim);
        }

        private void HandleMove()
        {
            animator.speed = 1f;
            animator.Play(moveAnim);
        }

        private void HandleAttack()
        {
            animator.Play(attackAnim);
        }

        // Called via Animation Event at the end of the Attack clip
        public void OnAttackComplete()
        {
            CurrentMoveState = EnemyMoveStateEnum.Idle;
        }
    }
}