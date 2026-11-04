using UnityEngine;

namespace GameObjects.Common.Terrain.Scripts
{
    // Starts the looping idle clip at a random point, so trees next to each other don't sway in sync
    [RequireComponent(typeof(Animator))]
    public class RandomAnimationStart : MonoBehaviour
    {
        private const string IdleAnim = "Idle";

        private Animator _animator;

        private void Awake() => _animator = GetComponent<Animator>();
        private void Start() => _animator.Play(IdleAnim, 0, Random.value);
    }
}
