using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Tile Sounds")]
        public AudioClip balloonPop;
        public AudioClip cubeCollect;
        public AudioClip cubeExplode;
        public AudioClip duckSound;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;

            audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip);
        }
    }
}
