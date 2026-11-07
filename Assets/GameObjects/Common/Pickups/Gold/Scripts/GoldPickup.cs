using GameObjects.Common.Pickups.Scripts;
using GameObjects.Common.Progression.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Common.Pickups.Gold.Scripts
{
    // Gold goes to the team, whoever picks it up
    public class GoldPickup : MonoBehaviour, IPickup
    {
        private int _amount;

        public void SetAmount(int amount) => _amount = amount;

        public void Collect(GameObject collector)
        {
            var team = TeamLevel.Current;
            if (team != null) team.AddGold(_amount);

            PooledObject.Release(gameObject);
        }
    }
}
