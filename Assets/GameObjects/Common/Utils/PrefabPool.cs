using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace GameObjects.Common.Utils
{
    // Reuses instances of short-lived prefabs (projectiles, damage numbers, dust, gold) instead of
    // instantiating and destroying one per shot, hit or death. Spawn replaces Instantiate, and
    // PooledObject.Release(gameObject) replaces Destroy. Pooled scripts reset their own state in OnEnable
    // or in the setup call their spawner makes, because a reused object keeps whatever it had last time
    public static class PrefabPool
    {
        private const int MaxPooledPerPrefab = 256;

        private static readonly Dictionary<GameObject, ObjectPool<PooledObject>> Pools = new();

        // Pooled objects live in the scene and die with it, so the pools start empty in every scene
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneUnloaded += _ => Pools.Clear();

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!Pools.TryGetValue(prefab, out var pool))
            {
                pool = CreatePool(prefab);
                Pools[prefab] = pool;
            }

            // Skip instances that died with a scene, whatever order the scene events come in
            PooledObject item;
            do item = pool.Get();
            while (item == null);

            // Placed before activation, so OnEnable already sees the right position
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item.gameObject;
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component =>
            Spawn(prefab.gameObject, position, rotation).GetComponent<T>();

        private static ObjectPool<PooledObject> CreatePool(GameObject prefab)
        {
            ObjectPool<PooledObject> pool = null;
            pool = new ObjectPool<PooledObject>(
                createFunc: () =>
                {
                    var instance = Object.Instantiate(prefab);
                    var pooled = instance.AddComponent<PooledObject>();
                    pooled.Pool = pool;
                    return pooled;
                },
                actionOnGet: pooled => pooled.InPool = false,
                actionOnRelease: pooled =>
                {
                    pooled.InPool = true;
                    pooled.gameObject.SetActive(false);
                },
                actionOnDestroy: pooled =>
                {
                    if (pooled != null) Object.Destroy(pooled.gameObject);
                },
                collectionCheck: false,
                maxSize: MaxPooledPerPrefab);
            return pool;
        }
    }
}
