using UnityEngine;

namespace WizardPunk
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        [SerializeField] private AudioSource audioSource;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SoundManager: AudioSource atau Clip kosong!");
            }
        }

        // --- FUNGSI BARU UNTUK MENGHENTIKAN SUARA SECARA INSTAN ---
        public void StopSound()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        // -----------------------------------------------------------
    }
}