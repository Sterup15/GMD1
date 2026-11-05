using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Common.Terrain.Scripts
{
    // Scatters a few map decorations (trees, bushes, ground patches) at run start, so the open grass doesn't all look the same
    public class TerrainScatter : MonoBehaviour
    {
        [Tooltip("Picked at random, one per placed piece")]
        [SerializeField] private GameObject[] prefabs;
        [Tooltip("Max pieces. Fewer are placed when minSpacing leaves no room in the area")]
        [SerializeField, Min(0)] private int count = 8;
        [Tooltip("World area the pieces are placed in")]
        [SerializeField] private Rect area = new(-15f, -9f, 30f, 25f);
        [Tooltip("Min distance between two pieces, which controls how many show on screen")]
        [SerializeField] private float minSpacing = 11f;
        [Tooltip("No pieces this close to where the player starts")]
        [SerializeField] private float playerClearRadius = 3f;
        [SerializeField] private LayerMask waterLayer = 1 << 4;
        [Tooltip("Min distance from the water, so no piece hangs over the coast")]
        [SerializeField] private float waterClearance = 1f;

        [Tooltip("Other scatters whose pieces these keep away from (e.g. bushes away from trees)")]
        [SerializeField] private TerrainScatter[] avoid;
        [Tooltip("Min distance from the avoided scatters' pieces")]
        [SerializeField, Min(0f)] private float avoidDistance = 2f;

        private const int MaxAttemptsPerPiece = 30;

        private readonly List<Vector2> _placed = new();
        private bool _done;

        private void Start() => EnsurePlaced();

        // Places the pieces once. A scatter that avoids this one calls it first, so the order of Start calls
        // between scatters doesn't matter
        public void EnsurePlaced()
        {
            if (_done) return;
            _done = true;

            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning("TerrainScatter: No prefabs assigned.", this);
                return;
            }

            if (avoid != null)
                foreach (var other in avoid)
                    if (other != null && other != this) other.EnsurePlaced();

            var playerObj = GameObject.FindWithTag("Player");
            Vector2 playerStart = playerObj != null ? playerObj.transform.position : Vector2.zero;

            for (int i = 0; i < count; i++)
            {
                if (!TryFindSpot(playerStart, _placed, out var position)) continue;

                _placed.Add(position);
                Instantiate(prefabs[Random.Range(0, prefabs.Length)], position, Quaternion.identity, transform);
            }
        }

        private bool TryFindSpot(Vector2 playerStart, List<Vector2> placed, out Vector2 position)
        {
            for (int attempt = 0; attempt < MaxAttemptsPerPiece; attempt++)
            {
                position = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
                if (IsFree(position, playerStart, placed)) return true;
            }

            position = default;
            return false;
        }

        private bool IsFree(Vector2 position, Vector2 playerStart, List<Vector2> placed)
        {
            if ((position - playerStart).sqrMagnitude < playerClearRadius * playerClearRadius) return false;
            if (Physics2D.OverlapCircle(position, waterClearance, waterLayer) != null) return false;

            foreach (var other in placed)
                if ((position - other).sqrMagnitude < minSpacing * minSpacing) return false;

            if (avoid != null)
                foreach (var scatter in avoid)
                {
                    if (scatter == null || scatter == this) continue;
                    foreach (var other in scatter._placed)
                        if ((position - other).sqrMagnitude < avoidDistance * avoidDistance) return false;
                }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(area.center, area.size);
        }
#endif
    }
}
