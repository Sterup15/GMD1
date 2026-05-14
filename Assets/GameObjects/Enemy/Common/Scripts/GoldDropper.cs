using GameObjects.Common.Events;
using GameObjects.Common.Gold.Scripts;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts
{
    public class GoldDropper : MonoBehaviour
    {
        [SerializeField] private GameObject goldPickupPrefab;

        private ActorEvents _events;
        private int _amount;

        private void Awake() => _events = GetComponent<ActorEvents>();
        private void OnEnable()  => _events.OnDeath += Drop;
        private void OnDisable() => _events.OnDeath -= Drop;

        public void SetAmount(int amount) => _amount = amount;

        private void Drop()
        {
            if (goldPickupPrefab == null || _amount <= 0) return;
            var go = Instantiate(goldPickupPrefab, transform.position, Quaternion.identity);
            go.GetComponent<GoldPickup>().SetAmount(_amount);
        }
    }
}
