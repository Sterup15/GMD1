using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;

// The box player arrows bounce off once they have Bounce: four walls around the camera's position, sized by a setting
// rather than by the screen (online, players' screens differ). Requires a dedicated layer on this GameObject
// (e.g. "CameraWalls"); on Start all Physics2D collisions for that layer are disabled, and arrows are let in when
// the player gets Bounce. The boss fight's walls are BossArena's, not these
namespace GameObjects.Common.Camera.Scripts
{
    public class CameraWalls : MonoBehaviour
    {
        [Tooltip("World size of the bounce box, centred on the camera. About one 16:9 screen at the normal zoom; meant to become a lobby setting")]
        [SerializeField] private Vector2 size = new(19.56f, 11f);

        private UnityEngine.Camera _camera;
        private BoxCollider2D _topWall;
        private BoxCollider2D _bottomWall;
        private BoxCollider2D _leftWall;
        private BoxCollider2D _rightWall;

        private const float WallThickness = 1f;
        private int _wallLayer;
        private Stat _playerBounce;

        private void Awake()
        {
            _camera = UnityEngine.Camera.main;
            _wallLayer = gameObject.layer;

            _topWall    = CreateWall("CameraWall_Top");
            _bottomWall = CreateWall("CameraWall_Bottom");
            _leftWall   = CreateWall("CameraWall_Left");
            _rightWall  = CreateWall("CameraWall_Right");
        }

        private void Start()
        {
            for (int i = 0; i < 32; i++)
                Physics2D.IgnoreLayerCollision(_wallLayer, i, true);

            // Player arrows only hit the walls once they can bounce, so arrows aimed at enemies past the screen edge still reach them
            var player = Players.Local;
            var stats = player != null ? player.GetComponent<Stats.Scripts.Stats>() : null;
            if (stats == null) return;
            _playerBounce = stats.Bounce;
            _playerBounce.OnValueChanged += OnPlayerBounceChanged;
            OnPlayerBounceChanged();
        }

        private void OnDestroy()
        {
            if (_playerBounce != null) _playerBounce.OnValueChanged -= OnPlayerBounceChanged;
        }

        private void OnPlayerBounceChanged()
        {
            if (_playerBounce.Value >= 1f)
                EnableCollisionWithLayer(LayerMask.NameToLayer("PlayerProjectile"));
        }

        private BoxCollider2D CreateWall(string wallName)
        {
            var go = new GameObject(wallName);
            go.transform.SetParent(transform);
            go.layer = _wallLayer;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            return go.AddComponent<BoxCollider2D>();
        }

        private void LateUpdate()
        {
            PositionWalls();
        }

        private void PositionWalls()
        {
            float halfHeight = size.y / 2f;
            float halfWidth  = size.x / 2f;
            Vector2 cam      = _camera.transform.position;
            float halfThick  = WallThickness / 2f;

            SetWall(_topWall,    cam + new Vector2(0f,               halfHeight + halfThick), new Vector2(halfWidth * 2f + WallThickness, WallThickness));
            SetWall(_bottomWall, cam + new Vector2(0f,              -halfHeight - halfThick), new Vector2(halfWidth * 2f + WallThickness, WallThickness));
            SetWall(_leftWall,   cam + new Vector2(-halfWidth - halfThick, 0f),               new Vector2(WallThickness, halfHeight * 2f));
            SetWall(_rightWall,  cam + new Vector2( halfWidth + halfThick, 0f),               new Vector2(WallThickness, halfHeight * 2f));
        }

        private static void SetWall(BoxCollider2D wall, Vector2 position, Vector2 size)
        {
            wall.transform.position = position;
            // Resizing rebuilds the collider shape, so only do it when the camera size or aspect changed
            if (wall.size != size)
                wall.size = size;
        }

        public void EnableCollisionWithLayer(int layer)
        {
            Physics2D.IgnoreLayerCollision(_wallLayer, layer, false);
        }
    }
}
