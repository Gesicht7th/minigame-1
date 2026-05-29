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
        [SerializeField] private GameObject absorbLightObject; // Objek Light (bisa berupa Point Light atau Game Object biasa)
        [SerializeField] private float absorbLightDuration = 0.5f; // Berapa lama light menyala
        [SerializeField] private float absorbLightMaxIntensity = 5f; // Intensitas maksimal saat paling cerah

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

        [Header("── Ult Absorb VFX ─────────────────────────────")]
        [SerializeField] private GameObject ultAbsorbVfxPrefab;
        [SerializeField] private Transform ultAbsorbSpawnPoint;

        private bool isUltAbsorbCondition = false;

        public void SetMatchCondition(bool isLoser, RpsType myAction, RpsType opponentAction)
        {
            // Kondisi: kalah saat menggunakan Paper melawan Scissors
            isUltAbsorbCondition = (isLoser && myAction == RpsType.Paper && opponentAction == RpsType.Scissors);
        }

        // Anda bisa menambahkan banyak VFX lain di sini nantinya
        // [Header("── Magic VFX ─────────────────────────────")]
        // [SerializeField] private GameObject fireballPrefab;
        // [SerializeField] private Transform fireballSpawnPoint;

        /// <summary>
        /// Dipanggil dari Animation Event pada klip animasi (misal: Attack)
        /// </summary>
        public void PlayStrikeVFX()
        {
            if (isUltAbsorbCondition)
            {
                PlayUltAbsorbVFX();
                return;
            }

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
            if (isUltAbsorbCondition) return;

            if (sparksVfxPrefab != null && sparksSpawnPoint != null)
            {
                Instantiate(sparksVfxPrefab, sparksSpawnPoint.position, sparksSpawnPoint.rotation);
            }
        }

        public void PlayUltAbsorbVFX()
        {
            if (ultAbsorbVfxPrefab != null && ultAbsorbSpawnPoint != null)
            {
                Instantiate(ultAbsorbVfxPrefab, ultAbsorbSpawnPoint.position, ultAbsorbSpawnPoint.rotation);
            }
        }

        public void PlayAbsorbVFX()
        {
            if (absorbVfxPrefab != null && absorbSpawnPoint != null)
            {
                Instantiate(absorbVfxPrefab, absorbSpawnPoint.position, absorbSpawnPoint.rotation, absorbSpawnPoint);
            }

            if (absorbLightObject != null)
            {
                StopCoroutine(nameof(FlashAbsorbLight));
                StartCoroutine(nameof(FlashAbsorbLight));
            }
        }

        private System.Collections.IEnumerator FlashAbsorbLight()
        {
            if (absorbLightObject == null) yield break;

            absorbLightObject.SetActive(true);
            
            // Mencari komponen Light di objek ini atau anaknya
            Light lightComp = absorbLightObject.GetComponent<Light>();
            if (lightComp == null) lightComp = absorbLightObject.GetComponentInChildren<Light>();

            if (lightComp != null)
            {
                float elapsed = 0f;
                while (elapsed < absorbLightDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / absorbLightDuration;
                    
                    // Efek Fade In: Intensitas naik perlahan dari 0 ke Max (redup menuju cerah)
                    lightComp.intensity = Mathf.Lerp(0f, absorbLightMaxIntensity, t);
                    
                    // Catatan: Jika ingin efek menyala lalu redup kembali secara halus, bisa gunakan:
                    // lightComp.intensity = Mathf.Lerp(0f, absorbLightMaxIntensity, Mathf.Sin(t * Mathf.PI));

                    yield return null;
                }
            }
            else
            {
                // Fallback jika tidak ada komponen Light (hanya GameObject biasa)
                yield return new WaitForSeconds(absorbLightDuration);
            }

            absorbLightObject.SetActive(false);
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
