using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ReadySoundTrigger : MonoBehaviour
    {
        [Header("── Audio Settings ──────────────────────")]
        [Tooltip("SFX saat Player 1 (Fura) klik Ready")]
        [SerializeField] private AudioClip p1ReadySFX;

        [Tooltip("SFX saat Player 2 (Oura) klik Ready")]
        [SerializeField] private AudioClip p2ReadySFX;

        [Header("── Optional: Auto-Detect UI Button ───────")]
        [SerializeField] private Button p1ReadyButton;
        [SerializeField] private Button p2ReadyButton;

        // Mencegah suara dimainkan berkali-kali jika pemain spam tombol
        private bool p1HasPlayed = false;
        private bool p2HasPlayed = false;

        void Start()
        {
            // Menghubungkan tombol UI (jika ada) ke fungsi suara
            if (p1ReadyButton != null)
                p1ReadyButton.onClick.AddListener(PlayP1ReadySound);

            if (p2ReadyButton != null)
                p2ReadyButton.onClick.AddListener(PlayP2ReadySound);
        }

        void Update()
        {
            // Deteksi Player 1 (Tombol Spasi)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayP1ReadySound();
            }

            // Deteksi Player 2 (Tombol Enter)
            // Menggunakan Return (Enter biasa) dan KeypadEnter (Enter di Numpad)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                PlayP2ReadySound();
            }
        }

        // --- FUNGSI UNTUK PLAYER 1 ---
        public void PlayP1ReadySound()
        {
            if (p1HasPlayed) return; // Hentikan jika sudah ready

            if (SoundManager.Instance != null && p1ReadySFX != null)
            {
                SoundManager.Instance.PlaySound(p1ReadySFX);
                p1HasPlayed = true;
                Debug.Log("[Ready Scene] Player 1 (Fura) Ready!");
            }
        }

        // --- FUNGSI UNTUK PLAYER 2 ---
        public void PlayP2ReadySound()
        {
            if (p2HasPlayed) return; // Hentikan jika sudah ready

            if (SoundManager.Instance != null && p2ReadySFX != null)
            {
                SoundManager.Instance.PlaySound(p2ReadySFX);
                p2HasPlayed = true;
                Debug.Log("[Ready Scene] Player 2 (Oura) Ready!");
            }
        }

        // Opsional: Jika game Anda punya fitur "Un-Ready" (Batal Ready), panggil fungsi ini
        public void ResetReadyState()
        {
            p1HasPlayed = false;
            p2HasPlayed = false;
        }
    }
}