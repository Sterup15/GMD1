using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.MeleeEnemy.Scripts.Behaviour
{
    public class EnemyMeleeAnimationEvents : MonoBehaviour
    {
        private ActorEvents _events;

        private void Awake() => _events = GetComponentInParent<ActorEvents>();

        public void OnAttackComplete() => _events.AttackComplete();
    }
}
