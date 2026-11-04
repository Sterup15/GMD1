using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Scripts.Movement
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
        private ActorEvents _events;
        private Vector2 _dashVelocity;
        private bool _isDashing;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
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

        private void StartDash(Vector2 velocity)
        {
            _dashVelocity = velocity;
            _isDashing = true;
        }

        // Continue at normal running speed instead of sliding out of the dash
        private void EndDash()
        {
            _isDashing = false;
            currentVelocity = inputDirection * stats.MoveSpeed.Value;
        }

        private void Update()
        {
            inputDirection = GameInput.Move;
            if (normalizeDiagonals && inputDirection.sqrMagnitude > 1f)
                inputDirection.Normalize();
        }

        private void FixedUpdate()
        {
            if (_isDashing)
            {
                rb.linearVelocity = _dashVelocity;
                return;
            }

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
