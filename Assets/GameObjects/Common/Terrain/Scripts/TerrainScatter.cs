using System.Collections.Generic;
using UnityEngine;

namespace GameObjects.Common.Terrain.Scripts
{
    // Scatters a few trees and stumps over the map at run start, as landmarks so the open grass doesn't all look the same
    public class TerrainScatter : MonoBehaviour
    {
        [Tooltip("Picked at random, one per placed piece")]
        [SerializeField] private GameObject[] prefabs;
        [Tooltip("Max pieces. Fewer are placed when minSpacing leaves no room in the area")]
        [SerializeField, Min(0)] private int count = 8;
        [Tooltip("World area the pieces are placed in")]
        [SerializeField] private Rect area = new(-15f, -9f, 30f, 25f);
        [Tooltip("Min distance between two pieces. Keeps it to about 2-3 on screen")]
        [SerializeField] private float minSpacing = 11f;
        [Tooltip("No pieces this close to where the player starts")]
        [SerializeField] private float playerClearRadius = 3f;
        [SerializeField] private LayerMask waterLayer = 1 << 4;
        [Tooltip("Min distance from the water, so no piece hangs over the coast")]
        [SerializeField] private float waterClearance = 1f;

        private const int MaxAttemptsPerPiece = 30;

        private void Start()
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning("TerrainScatter: No prefabs assigned.", this);
                return;
            }

            var playerObj = GameObject.FindWithTag("Player");
            Vector2 playerStart = playerObj != null ? playerObj.transform.position : Vector2.zero;

            var placed = new List<Vector2>(count);
            for (int i = 0; i < count; i++)
            {
                if (!TryFindSpot(playerStart, placed, out var position)) continue;

                placed.Add(position);
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
