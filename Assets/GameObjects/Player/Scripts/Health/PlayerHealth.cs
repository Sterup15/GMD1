using System;
using System.Collections;
using GameObjects.Common.Events;
using GameObjects.Common.Projectiles.Scripts;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public static Action<int, int> OnHealthChanged;

        [SerializeField] private string enemyLayerName = "Enemy";
        private const float InvulnerabilityDuration = 3f;
        private bool _isInvulnerable;
        private Coroutine _invulnerabilityCoroutine;

        private Stats _stats;
        private ActorEvents _events;

        private void Awake()
        {
            _stats = GetComponent<Stats>();
            _events = GetComponent<ActorEvents>();
            MaxHealth = Mathf.RoundToInt(_stats.MaxHealth.Value);
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void OnEnable()  => _stats.MaxHealth.OnValueChanged += OnMaxHealthUpgraded;
        private void OnDisable() => _stats.MaxHealth.OnValueChanged -= OnMaxHealthUpgraded;

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
            if (_isInvulnerable) return;

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

        private void BeginInvulnerability()
        {
            if (_invulnerabilityCoroutine != null)
                StopCoroutine(_invulnerabilityCoroutine);
            _invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            _isInvulnerable = true;
            int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);

            yield return new WaitForSeconds(InvulnerabilityDuration);

            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
            _isInvulnerable = false;
            _invulnerabilityCoroutine = null;
        }
    }
}
