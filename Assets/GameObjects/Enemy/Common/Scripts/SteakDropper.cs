using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts
{
    public class SteakDropper : MonoBehaviour
    {
        [SerializeField] private GameObject steakPickupPrefab;
        [Tooltip("Chance (0-1) to drop a steak on death")]
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 0.005f;

        private ActorEvents _events;

        private void Awake() => _events = GetComponent<ActorEvents>();
        private void OnEnable()  => _events.OnDeath += Drop;
        private void OnDisable() => _events.OnDeath -= Drop;

        private void Drop()
        {
            if (steakPickupPrefab == null || Random.value >= dropChance) return;
            Instantiate(steakPickupPrefab, transform.position, Quaternion.identity);
        }
    }
}
