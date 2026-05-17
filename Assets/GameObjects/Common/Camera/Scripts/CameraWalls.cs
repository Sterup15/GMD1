using GameObjects.Common.Events;
using UnityEngine;

// Requires a dedicated layer assigned to this GameObject (e.g. "CameraWalls").
// On Start, all Physics2D collisions for that layer are disabled.
// Call EnableCollisionWithLayer / DisableCollisionWithLayer to control what the walls block.
namespace GameObjects.Common.Camera.Scripts
{
    public class CameraWalls : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private BoxCollider2D _topWall;
        private BoxCollider2D _bottomWall;
        private BoxCollider2D _leftWall;
        private BoxCollider2D _rightWall;

        private const float WallThickness = 1f;
        private int _wallLayer;

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

            GlobalEvents.OnBossSpawned    += OnBossSpawned;
            GlobalEvents.OnBossDefeated   += OnBossDefeated;
            GlobalEvents.OnBounceUnlocked += OnBounceUnlocked;

            if (GlobalEvents.IsBounceUnlocked)
                OnBounceUnlocked();
        }

        private void OnDestroy()
        {
            GlobalEvents.OnBossSpawned    -= OnBossSpawned;
            GlobalEvents.OnBossDefeated   -= OnBossDefeated;
            GlobalEvents.OnBounceUnlocked -= OnBounceUnlocked;
        }

        private void OnBossSpawned()    => EnableCollisionWithLayer(LayerMask.NameToLayer("Player"));
        private void OnBossDefeated()   => DisableCollisionWithLayer(LayerMask.NameToLayer("Player"));
        private void OnBounceUnlocked() => EnableCollisionWithLayer(LayerMask.NameToLayer("PlayerProjectile"));

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
            float halfHeight = _camera.orthographicSize;
            float halfWidth  = halfHeight * _camera.aspect;
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
            wall.size = size;
        }

        public void EnableCollisionWithLayer(int layer)
        {
            Physics2D.IgnoreLayerCollision(_wallLayer, layer, false);
        }

        public void DisableCollisionWithLayer(int layer)
        {
            Physics2D.IgnoreLayerCollision(_wallLayer, layer, true);
        }
    }
}
