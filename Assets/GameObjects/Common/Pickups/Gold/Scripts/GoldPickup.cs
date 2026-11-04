using GameObjects.Common.Pickups.Scripts;
using GameObjects.Player.Shared.Scripts.Gold;
using UnityEngine;

namespace GameObjects.Common.Pickups.Gold.Scripts
{
    public class GoldPickup : MonoBehaviour, IPickup
    {
        private int _amount;

        public void SetAmount(int amount) => _amount = amount;

        public void Collect(GameObject collector)
        {
            if (collector.TryGetComponent<PlayerGold>(out var gold))
                gold.AddGold(_amount);

            Destroy(gameObject);
        }
    }
}
