using Actors.Common;
using Actors.Common.Gold;
using UnityEngine;

namespace Actors.Player.Scripts.Gold
{
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private LayerMask pickupLayer;

        private Stats _stats;
        private readonly Collider2D[] _hits = new Collider2D[32];

        private void Awake() => _stats = GetComponent<Stats>();

        private void Update()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, _stats.PickupRange.Value, _hits, pickupLayer);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i] != null && _hits[i].TryGetComponent<GoldPickup>(out var pickup))
                    pickup.Collect();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_stats == null) _stats = GetComponent<Stats>();
            if (_stats == null) return;
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _stats.PickupRange.Value);
        }
#endif
    }
}
