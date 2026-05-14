using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.RangedEnemy.Scripts.Behaviour
{
    public class EnemyRangedAnimationEvents : MonoBehaviour
    {
        private ActorEvents _events;

        private void Awake() => _events = GetComponentInParent<ActorEvents>();

        public void OnShotFired()     => _events.ShotFired();
        public void OnAttackComplete() => _events.AttackComplete();
    }
}
