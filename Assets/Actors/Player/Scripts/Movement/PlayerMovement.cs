using Actors.Common;
using UnityEngine;

namespace Actors.Player.Scripts.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        [SerializeField] private bool normalizeDiagonals = true;

        private Rigidbody2D rb;
        private Stats stats;
        private Vector2 inputDirection;
        private Vector2 currentVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
            stats = GetComponent<Stats>();
        }

        private void Update()
        {
            inputDirection = GameInput.Move;
            if (normalizeDiagonals && inputDirection.sqrMagnitude > 1f)
                inputDirection.Normalize();
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = inputDirection * stats.MoveSpeed.Value;

            if (inputDirection.sqrMagnitude > 0f)
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            else
                currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);

            rb.linearVelocity = currentVelocity;
        }

        public Vector2 MoveDirection => inputDirection;
        public bool IsMoving => inputDirection.sqrMagnitude > 0f;
        public void SetVelocity(Vector2 v) { currentVelocity = v; rb.linearVelocity = v; }
    }
}
