// Assets/_Game/Scripts/Audio/SoundManager.cs
using UnityEngine;

namespace WizardPunk
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [Header("── Audio Sources ──")]
        [Tooltip("Speaker untuk efek suara (SFX)")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Speaker khusus untuk lagu (BGM)")]
        [SerializeField] private AudioSource bgmSource;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                    bgmSource.playOnAwake = false;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ==========================================
        // ── FUNGSI SFX (EFEK SUARA) ──
        // ==========================================
        public void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SoundManager: AudioSource atau Clip SFX kosong!");
            }
        }

        public void StopSound()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (audioSource != null) audioSource.volume = Mathf.Clamp01(volume);
        }

        // ==========================================
        // ── FUNGSI BGM (LAGU) ──
        // ==========================================
        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;

            // Jika lagu yang diminta SAMA dengan lagu yang sedang berputar, biarkan berlanjut
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            // Jika lagunya berbeda, ganti dan putar
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void SetBGMVolume(float volume)
        {
            if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(volume);
        }
    }
}