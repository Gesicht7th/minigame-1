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

        [SerializeField] private GameObject absorbVfxPrefab;
        [SerializeField] private Transform absorbSpawnPoint;

        [Header("── Parry VFX ─────────────────────────────")]
        [SerializeField] private GameObject parryVfxPrefab;
        [SerializeField] private Transform parrySpawnPoint;
        [SerializeField] private GameObject parrySparksVfxPrefab;
        [SerializeField] private Transform parrySparksSpawnPoint;
        // [SerializeField] private GameObject parryAbsorbVfxPrefab;
        // [SerializeField] private Transform parryAbsorbSpawnPoint;

        [Header("── Shield VFX ─────────────────────────────")]
        [SerializeField] private GameObject shieldVfxPrefab;
        [SerializeField] private Transform shieldSpawnPoint;

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

        public void PlayAbsorbVFX()
        {
            if (absorbVfxPrefab != null && absorbSpawnPoint != null)
            {
                Instantiate(absorbVfxPrefab, absorbSpawnPoint.position, absorbSpawnPoint.rotation, absorbSpawnPoint);
            }
        }

        public void PlayParryVFX()
        {
            if (parryVfxPrefab != null && parrySpawnPoint != null)
            {
                Instantiate(parryVfxPrefab, parrySpawnPoint.position, parrySpawnPoint.rotation);
            }
        }
        public void PlayParrySparksVFX()
        {
            if (parrySparksVfxPrefab != null && parrySparksSpawnPoint != null)
            {
                Instantiate(parrySparksVfxPrefab, parrySparksSpawnPoint.position, parrySparksSpawnPoint.rotation);
            }
        }
        // public void PlayParryAbsorbVFX()
        // {
        //     if (parryAbsorbVfxPrefab != null && parryAbsorbSpawnPoint != null)
        //     {
        //         Instantiate(parryAbsorbVfxPrefab, parryAbsorbSpawnPoint.position, parryAbsorbSpawnPoint.rotation, parryAbsorbSpawnPoint);
        //     }
        // }
        public void PlayShieldVFX()
        {
            if (shieldVfxPrefab != null && shieldSpawnPoint != null)
            {
                Instantiate(shieldVfxPrefab, shieldSpawnPoint.position, Quaternion.identity, shieldSpawnPoint);
            }
        }
    }
}
