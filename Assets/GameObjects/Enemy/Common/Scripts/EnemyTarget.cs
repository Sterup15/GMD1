using GameObjects.Common.Events;
using GameObjects.Common.Utils;
using UnityEngine;

namespace GameObjects.Enemy.Common.Scripts
{
    // Makes this enemy a target for the player's auto-aim while it is alive and not running off at the boss
    public class EnemyTarget : MonoBehaviour
    {
        private ActorEvents _events;
        private bool _registered;

        private void Awake() => _events = GetComponent<ActorEvents>();

        private void OnEnable()
        {
            Register();
            if (_events != null) _events.OnRetreat += Unregister;
        }

        private void OnDisable()
        {
            Unregister();
            if (_events != null) _events.OnRetreat -= Unregister;
        }

        private void Register()
        {
            if (_registered) return;
            Targeting.AddEnemy(transform);
            _registered = true;
        }

        private void Unregister(Vector2 _) => Unregister();

        private void Unregister()
        {
            if (!_registered) return;
            Targeting.RemoveEnemy(transform);
            _registered = false;
        }
    }
}
