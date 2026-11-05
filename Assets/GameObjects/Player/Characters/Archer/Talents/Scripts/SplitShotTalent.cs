using GameObjects.Common.Events;
using GameObjects.Common.Projectiles.Scripts;
using UnityEngine;

namespace GameObjects.Player.Characters.Archer.Talents.Scripts
{
    // Archer talent: every few shots, extra arrows fan out on both sides of the aimed one
    public class SplitShotTalent : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("Every this many shots splits")]
        [SerializeField, Min(1)] private int everyNthShot = 5;
        [Tooltip("Extra arrows, alternating left and right of the aimed one")]
        [SerializeField, Min(1)] private int extraArrows = 2;
        [Tooltip("Degrees between neighbouring arrows")]
        [SerializeField] private float spreadAngle = 15f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;
        private int _shots;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()  => _events.OnProjectileFired += OnProjectileFired;
        private void OnDisable() => _events.OnProjectileFired -= OnProjectileFired;

        private void OnProjectileFired(Vector2 origin, Vector2 direction)
        {
            if (++_shots < everyNthShot) return;
            _shots = 0;

            for (int i = 1; i <= extraArrows; i++)
            {
                float angle = spreadAngle * ((i + 1) / 2) * (i % 2 == 1 ? 1f : -1f);
                Vector2 rotated = Quaternion.Euler(0f, 0f, angle) * direction;
                ProjectileSpawner.Spawn(projectilePrefab, origin, rotated, _stats);
            }
        }
    }
}
