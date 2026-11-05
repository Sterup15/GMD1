using GameObjects.Common.Pickups.Scripts;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Gold
{
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private LayerMask pickupLayer;

        private Stats _stats;
        private readonly Collider2D[] _hits = new Collider2D[32];
        private ContactFilter2D _filter;

        private void Awake()
        {
            _stats  = GetComponent<Stats>();
            _filter = new ContactFilter2D { useLayerMask = true, layerMask = pickupLayer, useTriggers = true };
        }

        private void Update()
        {
            int count = Physics2D.OverlapCircle(transform.position, _stats.PickupRange.Value, _filter, _hits);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i] != null && _hits[i].TryGetComponent<IPickup>(out var pickup))
                    pickup.Collect(gameObject);
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
