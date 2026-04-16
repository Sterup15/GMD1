using System;
using Actors.Common;
using Actors.Projectile.Scripts;
using UnityEngine;

namespace Actors.Player.Scripts.Movement
{
    public class PlayerMovementState : MonoBehaviour
    {
        public MoveStateEnum currentMoveState { get; private set; }
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        private ProjectileSpawner projectileSpawner;
        [SerializeField] private float shootAnimLength = 1f;
        private PlayerMovement playerMovement;
        private Stats stats;
        private const string idleAnim = "Idle";
        private const string moveAnim = "Move";
        private const string attackAnim = "Attack";
        public static Action<MoveStateEnum> OnPlayerMoveStateChanged;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            stats = GetComponent<Stats>();
            projectileSpawner = GetComponent<ProjectileSpawner>();
        }

        private void Update()
        {
            if (playerMovement.IsMoving)
                SetMoveState(MoveStateEnum.Move);
            else
                SetMoveState(NearestEnemyInRange() != null ? MoveStateEnum.Attack : MoveStateEnum.Idle);

            if (playerMovement.MoveDirection.x != 0f)
                spriteRenderer.flipX = playerMovement.MoveDirection.x < 0f;
        }

        private GameObject NearestEnemyInRange()
        {
            float range = stats.ShootRange.Value;
            float rangeSqr = range * range;
            GameObject nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                float dist = (enemy.transform.position - transform.position).sqrMagnitude;
                if (dist <= rangeSqr && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
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
                
                case MoveStateEnum.Attack:
                    HandleAttack ();
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

        // Called via Animation Event at the arrow-release frame of the shoot clip
        public void OnShotFired()
        {
            projectileSpawner.Fire();
        }
    }
}