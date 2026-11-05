using System;
using UnityEngine;

namespace GameObjects.Enemy.Spawner.Scripts
{
    [Serializable]
    public class BossSpawnConfig
    {
        public string name;
        public GameObject prefab;

        [Header("Gold Drop")]
        public float goldDrop = 100f;
    }
}
