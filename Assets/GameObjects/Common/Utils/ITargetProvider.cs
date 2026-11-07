using UnityEngine;

namespace GameObjects.Common.Utils
{
    // Picks what an actor goes after (EnemyTargetSelector on enemies), so shared code like ProjectileSpawner can aim
    // at it without knowing how it's chosen
    public interface ITargetProvider
    {
        // Null when there is nothing to go after
        Transform Target { get; }
    }
}
