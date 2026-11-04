using System;
using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Scripts.Dash
{
    public class PlayerDash : MonoBehaviour
    {
        [Header("Dash")]
        [Tooltip("Seconds to recharge one dash charge before Haste")]
        [SerializeField] private float baseCooldown = 4f;
        [Tooltip("Player speed while dashing, in units per second")]
        [SerializeField] private float dashSpeed = 18f;
        [Tooltip("How long a dash lasts, in seconds. The player is invulnerable for this time")]
        [SerializeField] private float dashDuration = 0.09f;

        private Stats _stats;
        private ActorEvents _events;
        private Vector2 _lastDirection = Vector2.right;
        private float _dashTimer;
        private float _rechargeTimer;
        private int _maxCharges;

        public int Charges { get; private set; }
        public int MaxCharges => Mathf.Max(1, Mathf.RoundToInt(_stats.DashCharges.Value));
        public float Cooldown => baseCooldown / (1f + _stats.Haste.Value / 100f);
        public float CooldownProgress => Charges >= MaxCharges ? 1f : Mathf.Clamp01(_rechargeTimer / Cooldown);
        private bool IsDashing => _dashTimer > 0f;

        public event Action<int, int> OnChargesChanged;

        private void Awake()
        {
            _stats  = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            _maxCharges = MaxCharges;
            Charges = _maxCharges;
        }

        private void OnEnable()  => _stats.DashCharges.OnValueChanged += OnMaxChargesChanged;
        private void OnDisable() => _stats.DashCharges.OnValueChanged -= OnMaxChargesChanged;

        // Charges gained from an upgrade are ready straight away
        private void OnMaxChargesChanged()
        {
            int newMax = MaxCharges;
            Charges = Mathf.Clamp(Charges + newMax - _maxCharges, 0, newMax);
            _maxCharges = newMax;
            OnChargesChanged?.Invoke(Charges, MaxCharges);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            if (GameInput.Move.sqrMagnitude > 0f)
                _lastDirection = GameInput.Move.normalized;

            Recharge();

            // No dash animation clip exists, so the dash length is a timer instead of an animation event
            if (IsDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f) _events.DashEnded();
                return;
            }

            if (GameInput.DashPressed && Charges > 0)
                StartDash();
        }

        private void StartDash()
        {
            Charges--;
            _dashTimer = dashDuration;
            OnChargesChanged?.Invoke(Charges, MaxCharges);
            _events.DashStarted(_lastDirection * dashSpeed);
        }

        private void Recharge()
        {
            if (Charges >= MaxCharges) return;

            _rechargeTimer += Time.deltaTime;
            if (_rechargeTimer < Cooldown) return;

            _rechargeTimer = 0f;
            Charges++;
            OnChargesChanged?.Invoke(Charges, MaxCharges);
        }
    }
}
