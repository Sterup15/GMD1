using GameObjects.Common.Pickups.Scripts;
using UnityEngine;

namespace GameObjects.Common.Pickups.Steak.Scripts
{
    public class SteakPickup : MonoBehaviour, IPickup
    {
        [SerializeField] private int healAmount = 1;

        public void Collect(GameObject collector)
        {
            // Left on the ground at full health so the player can come back for it later
            if (!collector.TryGetComponent<IHealable>(out var health) || health.IsFullHealth) return;

            health.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
