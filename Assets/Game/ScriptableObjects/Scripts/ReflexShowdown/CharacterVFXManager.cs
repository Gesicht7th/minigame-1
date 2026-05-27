using UnityEngine;

namespace WizardPunk.Reflex
{
    /// <summary>
    /// Script khusus untuk mengatur semua Particle System / VFX milik karakter.
    /// Script ini harus ditempelkan di GameObject yang SAMA dengan komponen Animator
    /// agar bisa dipanggil langsung oleh Animation Event.
    /// </summary>
    public class CharacterVFXManager : MonoBehaviour
    {
        [Header("── Melee VFX ─────────────────────────────")]
        [SerializeField] private GameObject strikeVfxPrefab;
        [SerializeField] private Transform strikeSpawnPoint;
        
        [SerializeField] private GameObject sparksVfxPrefab;
        [SerializeField] private Transform sparksSpawnPoint;

        // Anda bisa menambahkan banyak VFX lain di sini nantinya
        // [Header("── Magic VFX ─────────────────────────────")]
        // [SerializeField] private GameObject fireballPrefab;
        // [SerializeField] private Transform fireballSpawnPoint;

        /// <summary>
        /// Dipanggil dari Animation Event pada klip animasi (misal: Attack)
        /// </summary>
        public void PlayStrikeVFX()
        {
            if (strikeVfxPrefab != null && strikeSpawnPoint != null)
            {
                Instantiate(strikeVfxPrefab, strikeSpawnPoint.position, strikeSpawnPoint.rotation);
            }
        }

        /// <summary>
        /// Dipanggil dari Animation Event pada klip animasi
        /// </summary>
        public void PlaySparksVFX()
        {
            if (sparksVfxPrefab != null && sparksSpawnPoint != null)
            {
                Instantiate(sparksVfxPrefab, sparksSpawnPoint.position, sparksSpawnPoint.rotation);
            }
        }
    }
}
