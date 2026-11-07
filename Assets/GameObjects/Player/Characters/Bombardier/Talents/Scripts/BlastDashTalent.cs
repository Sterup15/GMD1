using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using GameObjects.Player.Shared.Scripts.Talents;
using UnityEngine;
using BombProjectile = GameObjects.Player.Characters.Bombardier.Bomb.Scripts.Bomb;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // Bombardier talent: dashing drops bombs that go off after a short fuse and catch the enemies that chased the
    // Bombardier there. Level 1 drops one where the dash started; higher levels spread more from the start to where
    // the dash ends, and from level 3 the count follows the distance dashed (which grows with Move Speed). Their blasts
    // are reported like a thrown bomb's, so bomb talents such as Cluster Bombs trigger too
    public class BlastDashTalent : MonoBehaviour, ILeveledTalent
    {
        [SerializeField] private BombProjectile bombPrefab;
        [Tooltip("Bombs dropped per dash at each talent level, level 1 first. From the distance level on this is the minimum")]
        [SerializeField] private int[] bombsPerLevel = { 1, 2, 5 };
        [Tooltip("From this talent level on, one bomb is dropped every bombSpacing units of the distance actually dashed")]
        [SerializeField, Min(1)] private int distanceFromLevel = 3;
        [SerializeField, Min(0.05f)] private float bombSpacing = 0.4f;
        [Tooltip("Seconds from the drop to the blast")]
        [SerializeField, Min(0f)] private float fuseTime = 1.5f;
        [Tooltip("Damage as a multiple of the Damage stat")]
        [SerializeField, Min(0f)] private float damageFactor = 1f;

        private ActorEvents _events;
        private Common.Stats.Scripts.Stats _stats;
        private Vector2 _dashStart;
        private int _level = 1;

        private void Awake()
        {
            _events = GetComponentInParent<ActorEvents>();
            _stats  = GetComponentInParent<Common.Stats.Scripts.Stats>();
        }

        private void OnEnable()
        {
            _events.OnDashStarted += RememberStart;
            _events.OnDashEnded   += DropBombs;
        }

        private void OnDisable()
        {
            _events.OnDashStarted -= RememberStart;
            _events.OnDashEnded   -= DropBombs;
        }

        public void SetLevel(int level) => _level = level;

        private void RememberStart(Vector2 _) => _dashStart = transform.position;

        private void DropBombs()
        {
            if (bombsPerLevel.Length == 0) return;
            Vector2 dashEnd = transform.position;
            int count = bombsPerLevel[Mathf.Clamp(_level - 1, 0, bombsPerLevel.Length - 1)];
            if (_level >= distanceFromLevel)
                count = Mathf.Max(count, Mathf.FloorToInt(Vector2.Distance(_dashStart, dashEnd) / bombSpacing) + 1);
            int damage = Mathf.Max(1, Mathf.RoundToInt(_stats.Damage.Value * damageFactor));

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0f : i / (count - 1f);
                var bomb = PrefabPool.Spawn(bombPrefab, Vector2.Lerp(_dashStart, dashEnd, t), Quaternion.identity);
                bomb.Drop(fuseTime, damage, _stats.Area.Value, _events.BombExploded);
            }
        }
    }
}
