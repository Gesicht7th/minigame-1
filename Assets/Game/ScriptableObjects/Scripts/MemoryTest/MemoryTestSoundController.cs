using UnityEngine;

namespace WizardPunk
{
    public class MemoryTestSoundController : MonoBehaviour
    {
        public static MemoryTestSoundController Instance;

        [Header("── Memory Test Audio Clips ────────────────")]
        [Tooltip("Suara saat panah menyala/muncul")]
        [SerializeField] private AudioClip arrowAppearSFX;

        [Tooltip("Suara saat batu bergerak/berputar")]
        [SerializeField] private AudioClip stoneMoveSFX;

        [Tooltip("Suara saat player salah menebak urutan")]
        [SerializeField] private AudioClip wrongGuessSFX;

        [Header("── UI Event Audio Clips ───────────────────")]
        [Tooltip("Suara saat pop-up Times Up muncul")]
        [SerializeField] private AudioClip timesUpSFX;

        [Tooltip("Suara saat pop-up Pemenang/Result muncul")]
        [SerializeField] private AudioClip resultSFX;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- FUNGSI UNTUK GAMEPLAY ---
        public void PlayArrowAppearSound()
        {
            if (SoundManager.Instance != null && arrowAppearSFX != null)
                SoundManager.Instance.PlaySound(arrowAppearSFX);
        }

        public void PlayStoneMoveSound()
        {
            if (SoundManager.Instance != null && stoneMoveSFX != null)
                SoundManager.Instance.PlaySound(stoneMoveSFX);
        }

        public void PlayWrongGuessSound()
        {
            if (SoundManager.Instance != null && wrongGuessSFX != null)
                SoundManager.Instance.PlaySound(wrongGuessSFX);
        }

        // --- FUNGSI BARU UNTUK UI EVENTS ---
        public void PlayTimesUpSound()
        {
            if (SoundManager.Instance != null && timesUpSFX != null)
                SoundManager.Instance.PlaySound(timesUpSFX);
        }

        public void PlayResultSound()
        {
            if (SoundManager.Instance != null && resultSFX != null)
                SoundManager.Instance.PlaySound(resultSFX);
        }
    }
}