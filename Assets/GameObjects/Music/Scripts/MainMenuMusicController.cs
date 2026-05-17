using UnityEngine;

namespace GameObjects.Music.Scripts
{
    public class MainMenuMusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip menuMusic;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (menuMusic == null) return;
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
