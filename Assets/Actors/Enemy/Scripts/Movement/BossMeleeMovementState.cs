using Actors.Enemy.Scripts.Behaviour;
using Actors.Enemy.Scripts.Health;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class BossMeleeMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private BossMelee _mover;
        private Rigidbody2D _rb;
        private EnemyPathfinder _pathfinder;
        private const string IdleAnim = "Idle";
        private const string MoveAnim = "Move";
        private const string WindupAnim = "Windup";
        private const string AttackAnim = "Attack";
        private const string RecoverAnim = "Recover";
        private const string DieAnim = "Die";

        private EnemyHealth _health;
        private BossMeleeAnimationEvents _animEvents;

        private void Awake()
        {
            _mover = GetComponent<BossMelee>();
            _rb = GetComponent<Rigidbody2D>();
            _pathfinder = GetComponent<EnemyPathfinder>();
            _health = GetComponent<EnemyHealth>();
            _animEvents = GetComponentInChildren<BossMeleeAnimationEvents>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnDeathStarted += OnDeathStarted;
            if (_animEvents != null) _animEvents.OnDeathAnimationComplete += _health.FinaliseDeath;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDeathStarted -= OnDeathStarted;
            if (_animEvents != null) _animEvents.OnDeathAnimationComplete -= _health.FinaliseDeath;
        }

        private void OnDeathStarted()
        {
            _pathfinder.Stop();
            _rb.linearVelocity = Vector2.zero;
            CurrentMoveState = EnemyMoveStateEnum.Die;
            animator.speed = 1f;
            animator.Play(DieAnim);
        }

        private void Update()
        {
            if (_mover.MoveDirection.x != 0f)
                spriteRenderer.flipX = _mover.MoveDirection.x < 0f;

            if (CurrentMoveState == EnemyMoveStateEnum.Windup ||
                CurrentMoveState == EnemyMoveStateEnum.Attack ||
                CurrentMoveState == EnemyMoveStateEnum.Recover ||
                CurrentMoveState == EnemyMoveStateEnum.Die) return;

            SetMoveState(_mover.IsMoving ? EnemyMoveStateEnum.Move : EnemyMoveStateEnum.Idle);
        }

        public void SetMoveState(EnemyMoveStateEnum newMoveState)
        {
            if (CurrentMoveState == newMoveState) return;

            switch (newMoveState)
            {
                case EnemyMoveStateEnum.Idle:
                    animator.speed = 1f;
                    animator.Play(IdleAnim);
                    break;
                case EnemyMoveStateEnum.Move:
                    _pathfinder.Resume();
                    animator.speed = 1f;
                    animator.Play(MoveAnim);
                    break;
                case EnemyMoveStateEnum.Windup:
                    _pathfinder.Stop();
                    animator.speed = 1f;
                    animator.Play(WindupAnim);
                    break;
                case EnemyMoveStateEnum.Attack:
                    animator.speed = 1f;
                    animator.Play(AttackAnim);
                    break;
                case EnemyMoveStateEnum.Recover:
                    _pathfinder.Stop();
                    animator.speed = 1f;
                    animator.Play(RecoverAnim);
                    break;
                case EnemyMoveStateEnum.Die:
                    _pathfinder.Stop();
                    _rb.linearVelocity = Vector2.zero;
                    animator.speed = 1f;
                    animator.Play(DieAnim);
                    break;
                default:
                    Debug.LogError($"Invalid move state: {newMoveState}");
                    break;
            }

            CurrentMoveState = newMoveState;
        }

        // Called via BossMeleeAnimationEvents at the end of the Windup clip
        public void OnWindupComplete()
        {
            SetMoveState(EnemyMoveStateEnum.Attack);
        }

        // Called via BossMeleeAnimationEvents at the end of the Attack clip
        public void OnAttackComplete()
        {
            SetMoveState(EnemyMoveStateEnum.Recover);
        }

        // Called via BossMeleeAnimationEvents at the end of the Recover clip
        public void OnRecoverComplete()
        {
            CurrentMoveState = EnemyMoveStateEnum.Idle;
        }
    }
}
