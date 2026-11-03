using UnityEngine;

namespace GameObjects.Common.Pickups.Scripts
{
    // Anything the player collects by having it inside their pickup range
    public interface IPickup
    {
        void Collect(GameObject collector);
    }
}
