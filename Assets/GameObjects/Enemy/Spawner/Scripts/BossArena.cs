using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Enemy.Spawner.Scripts
{
    // The space a boss fight happens in: a fixed rectangle centred on the host player, with walls that hold the players
    // in (enemies and the boss walk through them). Players outside it are moved in, and OnBossArenaOpened tells every
    // camera to lock onto it and zoom so all of it fits that screen. BossSpawner opens and closes it
    public class BossArena : MonoBehaviour
    {
        [Tooltip("World size of the arena. About one 16:9 screen at the normal zoom, so the camera hardly changes on a PC")]
        [SerializeField] private Vector2 size = new(19.5f, 11f);
        [SerializeField, Min(0.1f)] private float wallThickness = 1f;
        [Tooltip("A layer that collides only with the player layers")]
        [SerializeField] private string wallLayerName = "ArenaWalls";
        [Tooltip("Players further than this inside the edge count as already in the arena")]
        [SerializeField, Min(0f)] private float edgeMargin = 1f;
        [Tooltip("How far from the centre players moved into the arena are placed, spread around it")]
        [SerializeField, Min(0f)] private float gatherRadius = 1.5f;
        [SerializeField] private LayerMask obstacleLayer = 1 << 4;

        private GameObject _walls;
        private int _wallLayer;
        private BoxCollider2D _top, _bottom, _left, _right;

        public Vector2 Center { get; private set; }

        private void Awake()
        {
            // Without its layer a wall would block enemies and arrows too, so the arena then has no walls
            _wallLayer = LayerMask.NameToLayer(wallLayerName);
            if (_wallLayer < 0)
                Debug.LogError($"BossArena: Missing layer '{wallLayerName}' in the Tags and Layers settings, so the arena has no walls.", this);

            _walls = new GameObject("BossArenaWalls");
            _walls.transform.SetParent(transform, false);
            _top    = CreateWall("Top");
            _bottom = CreateWall("Bottom");
            _left   = CreateWall("Left");
            _right  = CreateWall("Right");
            _walls.SetActive(false);
        }

        // Opens the arena around the host and returns its centre, for spawning the boss relative to it
        public Vector2 Open(Vector2 hostPosition)
        {
            Center = hostPosition;
            PlaceWalls();
            _walls.SetActive(_wallLayer >= 0);
            GatherPlayers();
            GlobalEvents.BossArenaOpened(Center, size);
            return Center;
        }

        public void Close()
        {
            _walls.SetActive(false);
            GlobalEvents.BossArenaClosed();
        }

        private BoxCollider2D CreateWall(string wallName)
        {
            var go = new GameObject(wallName);
            go.transform.SetParent(_walls.transform, false);
            go.layer = Mathf.Max(0, _wallLayer);
            return go.AddComponent<BoxCollider2D>();
        }

        private void PlaceWalls()
        {
            Vector2 half = size / 2f;
            float t = wallThickness;
            SetWall(_top,    Center + new Vector2(0f,  half.y + t / 2f), new Vector2(size.x + t * 2f, t));
            SetWall(_bottom, Center + new Vector2(0f, -half.y - t / 2f), new Vector2(size.x + t * 2f, t));
            SetWall(_left,   Center + new Vector2(-half.x - t / 2f, 0f), new Vector2(t, size.y));
            SetWall(_right,  Center + new Vector2( half.x + t / 2f, 0f), new Vector2(t, size.y));
        }

        private static void SetWall(BoxCollider2D wall, Vector2 position, Vector2 wallSize)
        {
            wall.transform.position = position;
            wall.size = wallSize;
        }

        // Online, each owner will apply its own teleport; in solo the host is the centre, so nobody moves
        private void GatherPlayers()
        {
            var players = Players.All;
            Vector2 inner = size / 2f - Vector2.one * edgeMargin;
            for (int i = 0; i < players.Count; i++)
            {
                Vector2 offset = (Vector2)players[i].position - Center;
                if (Mathf.Abs(offset.x) < inner.x && Mathf.Abs(offset.y) < inner.y) continue;

                float angle = Mathf.PI * 2f * i / players.Count;
                Vector2 spot = Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * gatherRadius;
                if (Physics2D.OverlapPoint(spot, obstacleLayer) != null) spot = Center;

                if (players[i].TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.position = spot;
                    rb.linearVelocity = Vector2.zero;
                }
                players[i].position = spot;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || _walls == null || !_walls.activeSelf) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Center, size);
        }
#endif
    }
}
