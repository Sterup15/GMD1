using UnityEngine.Pool;
using UnityEngine;

namespace GameObjects.Common.Utils
{
    // Added by PrefabPool to every instance it creates, so the instance knows where to go back to
    public class PooledObject : MonoBehaviour
    {
        internal ObjectPool<PooledObject> Pool;
        internal bool InPool;

        // Returns a pooled instance to its pool, or destroys anything that wasn't spawned from one.
        // Releasing twice (e.g. a projectile that hits something on the frame its lifetime ends) is ignored
        public static void Release(GameObject instance)
        {
            if (instance.TryGetComponent<PooledObject>(out var pooled) && pooled.Pool != null)
            {
                if (!pooled.InPool) pooled.Pool.Release(pooled);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}
