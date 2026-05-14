using GameObjects.Common.Events;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using UnityEngine;

namespace GameObjects.Enemy.TogreBoss.Scripts.Behaviour
{
    public class BossMeleeMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private BossMelee _mover;
        private Rigidbody2D _rb;
        private EnemyPathfinder _pathfinder;
        private ActorEvents _events;
        private const string IdleAnim = "Idle";
        private const string MoveAnim = "Move";
        private const string WindupAnim = "Windup";
        private const string AttackAnim = "Attack";
        private const string RecoverAnim = "Recover";
        private const string DieAnim = "Die";

        private void Awake()
        {
            _mover = GetComponent<BossMelee>();
            _rb = GetComponent<Rigidbody2D>();
            _pathfinder = GetComponent<EnemyPathfinder>();
            _events = GetComponent<ActorEvents>();
        }

        private void OnEnable()
        {
            _events.OnDeathStarted   += OnDeathStarted;
            _events.OnWindupComplete  += WindupComplete;
            _events.OnAttackComplete  += AttackComplete;
            _events.OnRecoverComplete += RecoverComplete;
        }

        private void OnDisable()
        {
            _events.OnDeathStarted   -= OnDeathStarted;
            _events.OnWindupComplete  -= WindupComplete;
            _events.OnAttackComplete  -= AttackComplete;
            _events.OnRecoverComplete -= RecoverComplete;
        }

        private void OnDeathStarted()
        {
            _pathfinder.Stop();
            _rb.linearVelocity = Vector2.zero;
            CurrentMoveState = EnemyMoveStateEnum.Die;
            animator.speed = 1f;
            animator.Play(DieAnim);
        }

        private void WindupComplete()  => SetMoveState(EnemyMoveStateEnum.Attack);
        private void AttackComplete()  => SetMoveState(EnemyMoveStateEnum.Recover);
        private void RecoverComplete() => CurrentMoveState = EnemyMoveStateEnum.Idle;

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
    }
}
