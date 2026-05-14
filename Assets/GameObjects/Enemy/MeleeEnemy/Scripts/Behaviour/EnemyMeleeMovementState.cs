using System;
using GameObjects.Common.Events;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using UnityEngine;

namespace GameObjects.Enemy.MeleeEnemy.Scripts.Behaviour
{
    public class EnemyMeleeMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private EnemyMelee mover;
        private ActorEvents _events;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string attackAnim = "Attack";
        public static Action<EnemyMoveStateEnum> OnEnemyMoveStateChanged;

        private void Awake()
        {
            mover = GetComponent<EnemyMelee>();
            _events = GetComponent<ActorEvents>();
        }

        private void OnEnable()  => _events.OnAttackComplete += ResetToIdle;
        private void OnDisable() => _events.OnAttackComplete -= ResetToIdle;

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

        private void ResetToIdle() => CurrentMoveState = EnemyMoveStateEnum.Idle;

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
    }
}
