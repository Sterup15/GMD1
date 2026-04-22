using Actors.Common;
using Actors.Enemy.Scripts.Behaviour;
using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Enemy.Scripts
{
    public class EnemyRangedMovementState : MonoBehaviour
    {
        public EnemyMoveStateEnum CurrentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private ProjectileSpawner projectileSpawner;
        private EnemyRanged mover;
        private Stats stats;
        private Transform player;
        private bool isAttacking;
        public bool IsAttacking => isAttacking;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string attackAnim = "Attack";

        private void Awake()
        {
            mover = GetComponent<EnemyRanged>();
            projectileSpawner = GetComponent<ProjectileSpawner>();
            stats = GetComponent<Stats>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        private void Update()
        {
            if (mover.IsMoving)
                SetMoveState(EnemyMoveStateEnum.Move);
            else if (IsPlayerInRange())
                SetMoveState(EnemyMoveStateEnum.Attack);
            else
                SetMoveState(EnemyMoveStateEnum.Idle);

            if (mover.MoveDirection.x != 0f)
                spriteRenderer.flipX = mover.MoveDirection.x < 0f;
        }

        private bool IsPlayerInRange()
        {
            if (player == null) return false;
            float range = stats != null ? stats.ShootRange.Value : 6f;
            return Vector2.Distance(transform.position, player.position) <= range;
        }

        public void SetMoveState(EnemyMoveStateEnum newMoveState)
        {
            if (CurrentMoveState == newMoveState) return;
            if (isAttacking) return;

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
            isAttacking = true;
            if (mover.MoveDirection.x != 0f)
                spriteRenderer.flipX = mover.MoveDirection.x < 0f;

            animator.speed = mover.AttackAnimSpeed;
            animator.Play(attackAnim);
        }

        // Called via Animation Event at the shot-release frame of the Attack clip
        public void OnShotFired()
        {
            projectileSpawner.Fire();
        }

        // Called via Animation Event at the end of the Attack clip
        public void OnAttackComplete()
        {
            isAttacking = false;
        }

    }
}
