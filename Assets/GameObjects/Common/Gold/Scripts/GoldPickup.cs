using GameObjects.Player.Scripts.Gold;
using UnityEngine;

namespace GameObjects.Common.Gold.Scripts
{
    public class GoldPickup : MonoBehaviour
    {
        private int _amount;

        public void SetAmount(int amount) => _amount = amount;

        public void Collect()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null && playerObj.TryGetComponent<PlayerGold>(out var gold))
                gold.AddGold(_amount);

            Destroy(gameObject);
        }
    }
}
