using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Enemy.Common.Scripts.Behaviour;
using UnityEngine;

namespace GameObjects.Enemy.RangedEnemy.Scripts.Behaviour
{
    public class EnemyRangedMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private EnemyRanged _mover;
        private Stats _stats;
        private Transform _player;
        private ActorEvents _events;
        private bool isAttacking;

        public bool IsAttacking => isAttacking;

        private const string IdleAnim   = "Idle";
        private const string MoveAnim   = "Move";
        private const string AttackAnim = "Attack";

        private void Awake()
        {
            _mover  = GetComponent<EnemyRanged>();
            _stats  = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        private void OnEnable()
        {
            _events.OnShotFired      += FireProjectile;
            _events.OnAttackComplete += OnAttackComplete;
        }

        private void OnDisable()
        {
            _events.OnShotFired      -= FireProjectile;
            _events.OnAttackComplete -= OnAttackComplete;
        }

        private void Update()
        {
            if (isAttacking) return;

            if (IsPlayerInRange())
                SetMoveState(EnemyMoveStateEnum.Attack);
            else if (_player != null)
                SetMoveState(EnemyMoveStateEnum.Move);
            else
                SetMoveState(EnemyMoveStateEnum.Idle);

            if (_mover.MoveDirection.x != 0f)
                spriteRenderer.flipX = _mover.MoveDirection.x < 0f;
        }

        private void OnAttackComplete()
        {
            isAttacking = false;
            CurrentMoveState = EnemyMoveStateEnum.Idle;
        }

        private void FireProjectile()
        {
            GetComponent<GameObjects.Common.Projectiles.Scripts.ProjectileSpawner>().Fire();
        }

        private bool IsPlayerInRange()
        {
            if (_player == null) return false;
            float range = _stats != null ? _stats.ShootRange.Value : 6f;
            return Vector2.Distance(transform.position, _player.position) < range;
        }

        private void SetMoveState(EnemyMoveStateEnum newState)
        {
            if (CurrentMoveState == newState) return;

            switch (newState)
            {
                case EnemyMoveStateEnum.Idle:
                    animator.speed = 1f;
                    animator.Play(IdleAnim);
                    break;
                case EnemyMoveStateEnum.Move:
                    animator.speed = 1f;
                    animator.Play(MoveAnim);
                    break;
                case EnemyMoveStateEnum.Attack:
                    isAttacking = true;
                    animator.speed = _mover.AttackAnimSpeed;
                    animator.Play(AttackAnim);
                    if (_mover.MoveDirection.x != 0f)
                        spriteRenderer.flipX = _mover.MoveDirection.x < 0f;
                    break;
            }

            CurrentMoveState = newState;
        }
    }
}
