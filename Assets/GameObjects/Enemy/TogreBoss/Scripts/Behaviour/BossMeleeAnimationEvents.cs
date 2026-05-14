using GameObjects.Common.Events;
using UnityEngine;

namespace GameObjects.Enemy.TogreBoss.Scripts.Behaviour
{
    public class BossMeleeAnimationEvents : MonoBehaviour
    {
        private ActorEvents _events;

        private void Awake() => _events = GetComponentInParent<ActorEvents>();

        public void OnHitboxOpen()     => _events.HitboxOpen();
        public void OnHitboxClose()    => _events.HitboxClose();
        public void OnWindupComplete() => _events.WindupComplete();
        public void OnAttackComplete() => _events.AttackComplete();
        public void OnRecoverComplete() => _events.RecoverComplete();
        public void OnDeathComplete()  => _events.DeathComplete();
    }
}
