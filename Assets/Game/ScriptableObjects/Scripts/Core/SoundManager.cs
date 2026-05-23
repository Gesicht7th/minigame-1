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

                // --- KEAMANAN OTOMATIS ---
                // Jika AudioSource belum di-assign di Inspector, ambil komponen yang ada di objek ini
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
            // Cek sekali lagi sebelum memutar
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SoundManager: AudioSource atau Clip kosong!");
            }
        }
    }
}