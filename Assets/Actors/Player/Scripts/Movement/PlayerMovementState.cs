using System;
using UnityEngine;

namespace Actors.Player.Scripts.Movement
{
    public class PlayerMovementState : MonoBehaviour
    {
        public MoveStateEnum currentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        private PlayerMovement playerMovement;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string shootAnim = "Shoot";
        public static Action<MoveStateEnum> OnPlayerMoveStateChanged;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (currentMoveState == MoveStateEnum.Shoot) return;

            SetMoveState(playerMovement.IsMoving ? MoveStateEnum.Move : MoveStateEnum.Idle);

            if (playerMovement.MoveDirection.x != 0f)
                spriteRenderer.flipX = playerMovement.MoveDirection.x < 0f;
        }

        public void SetMoveState(MoveStateEnum newMoveState)
        {
            if (currentMoveState == newMoveState) return;

            switch (newMoveState)
            {
                case MoveStateEnum.Idle:
                    HandleIdle();
                    break;
                
                case MoveStateEnum.Move:
                    HandleMove();
                    break;
                
                case MoveStateEnum.Shoot:
                    HandleShoot ();
                    break;
                
                default:
                    Debug.LogError($"Invalid move state: {newMoveState}");
                    break;
            }
            
            OnPlayerMoveStateChanged?.Invoke(newMoveState);
            currentMoveState = newMoveState;
        }

        private void HandleShoot()
        {
            animator.Play(shootAnim);
        }

        private void HandleMove()
        {
            animator.Play(moveAnim);
        }

        private void HandleIdle()
        {
            animator.Play(idleAnim);
        }
    }
}