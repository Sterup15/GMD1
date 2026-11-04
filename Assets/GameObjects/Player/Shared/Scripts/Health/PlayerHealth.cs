using System;
using System.Collections;
using GameObjects.Common.Events;
using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsFullHealth => CurrentHealth >= MaxHealth;
        public static Action<int, int> OnHealthChanged;

        [SerializeField] private string enemyLayerName = "Enemy";
        [SerializeField] private string enemyProjectileLayerName = "EnemyProjectile";
        private const float InvulnerabilityDuration = 2f;
        private bool _isInvulnerable;
        private bool _isDashing;
        private int _enemyLayer;
        private int _enemyProjectileLayer;
        private Coroutine _invulnerabilityCoroutine;

        private Stats _stats;
        private ActorEvents _events;

        private void Awake()
        {
            _stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            _enemyLayer = LayerMask.NameToLayer(enemyLayerName);
            _enemyProjectileLayer = LayerMask.NameToLayer(enemyProjectileLayerName);
            MaxHealth = Mathf.RoundToInt(_stats.MaxHealth.Value);
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void OnEnable()
        {
            _stats.MaxHealth.OnValueChanged += OnMaxHealthUpgraded;
            _events.OnDashStarted           += StartDashInvulnerability;
            _events.OnDashEnded             += EndDashInvulnerability;
        }

        private void OnDisable()
        {
            _stats.MaxHealth.OnValueChanged -= OnMaxHealthUpgraded;
            _events.OnDashStarted           -= StartDashInvulnerability;
            _events.OnDashEnded             -= EndDashInvulnerability;

            // Layer collision settings survive scene loads, so restore them if a run ends mid-invulnerability
            Physics2D.IgnoreLayerCollision(gameObject.layer, _enemyLayer, false);
            Physics2D.IgnoreLayerCollision(gameObject.layer, _enemyProjectileLayer, false);
            _isInvulnerable = false;
            _isDashing = false;
            _invulnerabilityCoroutine = null;
        }

        private void OnMaxHealthUpgraded()
        {
            int newMax = Mathf.RoundToInt(_stats.MaxHealth.Value);
            int diff = newMax - MaxHealth;
            MaxHealth = newMax;
            CurrentHealth = Mathf.Min(CurrentHealth + diff, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;
            if (_isInvulnerable || _isDashing) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth > 0)
            {
                _events.Hit();
                BeginInvulnerability();
            }

            if (CurrentHealth == 0)
            {
                _events.Death();
                GlobalEvents.PlayerDied();
            }
        }

        public void Heal(int amount)
        {
            if (CurrentHealth <= 0 || IsFullHealth) return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            _events.Healed();
        }

        // Arrows fly through a dashing player instead of hitting it for no damage
        private void StartDashInvulnerability(Vector2 _)
        {
            _isDashing = true;
            UpdateEnemyCollision();
            Physics2D.IgnoreLayerCollision(gameObject.layer, _enemyProjectileLayer, true);
        }

        private void EndDashInvulnerability()
        {
            _isDashing = false;
            UpdateEnemyCollision();
            Physics2D.IgnoreLayerCollision(gameObject.layer, _enemyProjectileLayer, false);
        }

        // Hit i-frames and the dash can overlap, so enemies only collide again once both are over
        private void UpdateEnemyCollision()
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, _enemyLayer, _isInvulnerable || _isDashing);
        }

        private void BeginInvulnerability()
        {
            if (_invulnerabilityCoroutine != null)
                StopCoroutine(_invulnerabilityCoroutine);
            _invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            _isInvulnerable = true;
            UpdateEnemyCollision();
            _events.InvulnerabilityStarted();

            yield return new WaitForSeconds(InvulnerabilityDuration);

            _isInvulnerable = false;
            UpdateEnemyCollision();
            _invulnerabilityCoroutine = null;
            _events.InvulnerabilityEnded();
        }
    }
}
