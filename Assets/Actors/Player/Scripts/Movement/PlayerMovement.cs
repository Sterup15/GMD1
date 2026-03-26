using Actors.Common;
using UnityEngine;
using UnityEngine.InputSystem;

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
            inputDirection = Keyboard.current != null ? new Vector2(
                (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1 : 0) -
                (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed  ? 1 : 0),
                (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed    ? 1 : 0) -
                (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed  ? 1 : 0)
            ) : Vector2.zero;

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
