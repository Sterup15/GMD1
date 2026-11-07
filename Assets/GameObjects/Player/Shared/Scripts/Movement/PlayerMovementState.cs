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

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
        }

        private void OnEnable()
        {
            _events.OnDashStarted       += StartDash;
            _events.OnDashEnded         += EndDash;
            stats.FireRate.OnValueChanged += UpdateAttackSpeed;
        }

        private void OnDisable()
        {
            _events.OnDashStarted       -= StartDash;
            _events.OnDashEnded         -= EndDash;
            stats.FireRate.OnValueChanged -= UpdateAttackSpeed;
        }

        // Attack speed can change mid-volley (talents, timed buffs), so a running attack follows it at once
        private void UpdateAttackSpeed()
        {
            if (currentMoveState == MoveStateEnum.Attack)
                animator.speed = shootAnimLength * stats.FireRate.Value;
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

        private Transform NearestEnemyInRange() =>
            Targeting.NearestEnemy(transform.position, stats.ShootRange.Value);

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

            var previous = currentMoveState;
            currentMoveState = newMoveState;

            // Raised after the state is set, so an attack speed change from a listener applies to the new attack
            if ((previous == MoveStateEnum.Attack) != (newMoveState == MoveStateEnum.Attack))
                _events.AttackingChanged(newMoveState == MoveStateEnum.Attack);
        }

        private void HandleAttack()
        {
            var target = NearestEnemyInRange();
            if (target != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

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
