using GameObjects.Common.Events;
using UnityEngine;
using BombProjectile = GameObjects.Player.Characters.Bombardier.Bomb.Scripts.Bomb;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: dashing drops a bomb where the dash started, which goes off after a short fuse
    // and catches the enemies that chased the Bombardier there. Its blast is reported like a thrown bomb's,
    // so bomb talents such as Cluster Bombs trigger on it too
    public class BlastDashTalent : MonoBehaviour
    {
        [SerializeField] private BombProjectile bombPrefab;
        [Tooltip("Seconds from the drop to the blast")]
        [SerializeField, Min(0f)] private float fuseTime = 1.5f;
        [Tooltip("Damage as a multiple of the Damage stat")]
        [SerializeField, Min(0f)] private float damageFactor = 1f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()  => _events.OnDashStarted += Blast;
        private void OnDisable() => _events.OnDashStarted -= Blast;

        private void Blast(Vector2 _)
        {
            var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            bomb.Drop(fuseTime, Mathf.Max(1, Mathf.RoundToInt(_stats.Damage.Value * damageFactor)), _stats.Area.Value, _events.BombExploded);
        }
    }
}
