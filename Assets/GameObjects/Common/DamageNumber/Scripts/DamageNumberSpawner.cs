using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Common.DamageNumber.Scripts
{
    // Pops a floating number on every OnDamaged of its actor
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private DamageNumber damageNumberPrefab;

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()  => _events.OnDamaged += Spawn;
        private void OnDisable() => _events.OnDamaged -= Spawn;

        private void Spawn(int amount)
        {
            if (damageNumberPrefab == null) return;
            Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).Setup(amount);
        }
    }
}
