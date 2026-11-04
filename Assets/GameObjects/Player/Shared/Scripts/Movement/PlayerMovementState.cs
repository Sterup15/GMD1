using System;
using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Movement
{
    public class PlayerMovementState : MonoBehaviour
    {
        public MoveStateEnum currentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float shootAnimLength = 1f;
        private PlayerMovement playerMovement;
        private Stats stats;
        private ActorEvents _events;
        private bool _isDashing;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string attackAnim = "Attack";
        public static Action<MoveStateEnum> OnPlayerMoveStateChanged;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
        }

        private void OnEnable()
        {
            _events.OnDashStarted += StartDash;
            _events.OnDashEnded   += EndDash;
        }

        private void OnDisable()
        {
            _events.OnDashStarted -= StartDash;
            _events.OnDashEnded   -= EndDash;
        }

        // A dash from standing still has no move input, so face the dash and play the run clip instead of attacking
        private void StartDash(Vector2 velocity)
        {
            _isDashing = true;
            SetMoveState(MoveStateEnum.Move);
            if (velocity.x != 0f) spriteRenderer.flipX = velocity.x < 0f;
        }

        private void EndDash() => _isDashing = false;

        private void Update()
        {
            if (_isDashing) return;

            if (playerMovement.IsMoving)
                SetMoveState(MoveStateEnum.Move);
            else
                SetMoveState(NearestEnemyInRange() != null ? MoveStateEnum.Attack : MoveStateEnum.Idle);

            if (playerMovement.MoveDirection.x != 0f)
                spriteRenderer.flipX = playerMovement.MoveDirection.x < 0f;
        }

        private GameObject NearestEnemyInRange() =>
            Targeting.Nearest("Enemy", transform.position, stats.ShootRange.Value);

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

                case MoveStateEnum.Attack:
                    HandleAttack();
                    break;

                default:
                    Debug.LogError($"Invalid move state: {newMoveState}");
                    break;
            }

            OnPlayerMoveStateChanged?.Invoke(newMoveState);
            currentMoveState = newMoveState;
        }

        private void HandleAttack()
        {
            var target = NearestEnemyInRange();
            if (target != null)
                spriteRenderer.flipX = target.transform.position.x < transform.position.x;

            animator.speed = shootAnimLength * stats.FireRate.Value;
            animator.Play(attackAnim);
        }

        private void HandleMove()
        {
            animator.speed = 1f;
            animator.Play(moveAnim);
        }

        private void HandleIdle()
        {
            animator.speed = 1f;
            animator.Play(idleAnim);
        }
    }
}
