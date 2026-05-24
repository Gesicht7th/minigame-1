using UnityEngine;

namespace WizardPunk.HyperSmash
{
    // Baris ini akan otomatis menambahkan komponen AudioSource ke objek Anda
    [RequireComponent(typeof(AudioSource))]
    public class HyperSmashSoundController : MonoBehaviour
    {
        public static HyperSmashSoundController Instance;

        [Header("── In-Game Audio Clips ──────────────────")]
        [SerializeField] private AudioClip p1ShootSFX;
        [SerializeField] private AudioClip p2ShootSFX;
        [SerializeField] private AudioClip crystalDestroySFX;

        [Header("── UI Event Audio Clips ─────────────────")]
        [SerializeField] private AudioClip timesUpSFX;
        [SerializeField] private AudioClip resultSFX;

        // Kita gunakan AudioSource lokal khusus untuk scene ini
        private AudioSource sfxSource;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            // Ambil speaker lokal yang ada di objek ini
            sfxSource = GetComponent<AudioSource>();

            // Paksa suara menjadi 2D penuh agar suara selalu jelas 
            // tidak peduli seberapa cepat dan jauh kamera terbang
            sfxSource.spatialBlend = 0f;
            sfxSource.playOnAwake = false;
        }

        // --- MENGGUNAKAN PLAYONESHOT AGAR SUARA BISA BERTUMPUK (OVERLAP) ---

        public void PlayP1ShootSound()
        {
            if (p1ShootSFX != null) sfxSource.PlayOneShot(p1ShootSFX);
        }

        public void PlayP2ShootSound()
        {
            if (p2ShootSFX != null) sfxSource.PlayOneShot(p2ShootSFX);
        }

        public void PlayCrystalDestroySound()
        {
            if (crystalDestroySFX != null) sfxSource.PlayOneShot(crystalDestroySFX);
        }

        public void PlayTimesUpSound()
        {
            if (timesUpSFX != null) sfxSource.PlayOneShot(timesUpSFX);
        }

        public void PlayResultSound()
        {
            if (resultSFX != null) sfxSource.PlayOneShot(resultSFX);
        }
    }
}