// Assets/_Game/Scripts/HyperSmash/Projectile.cs
// ─────────────────────────────────────────────────────────────
// PERUBAHAN DARI VERSI SEBELUMNYA:
//   - Initialize() sekarang menerima parameter PlayerIndex
//   - OnTriggerEnter menyertakan ownerPlayer saat RegisterHit
//   - Crystal.TakeDamage juga diberitahu siapa yang menembak
// ─────────────────────────────────────────────────────────────

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class Projectile : MonoBehaviour
    {
        #region Config
        private float speed;
        private float maxRange;
        private Vector3 direction;
        private float traveledDistance = 0f;
        private PlayerIndex ownerPlayer = PlayerIndex.Player1;
        #endregion

        #region Public Properties
        public PlayerIndex Owner => ownerPlayer;
        #endregion

        #region Setup
        /// <summary>
        /// Initialize projectile dengan arah, kecepatan, jangkauan, dan siapa yang menembak.
        /// </summary>
        public void Initialize(Vector3 dir, float projectileSpeed, float range, PlayerIndex owner)
        {
            direction = dir.normalized;
            speed = projectileSpeed;
            maxRange = range;
            ownerPlayer = owner;
        }

        /// <summary>
        /// Overload tanpa owner (default Player1) — menjaga kompatibilitas jika ada code lama.
        /// </summary>
        public void Initialize(Vector3 dir, float projectileSpeed, float range)
        {
            Initialize(dir, projectileSpeed, range, PlayerIndex.Player1);
        }
        #endregion

        #region Unity Lifecycle
        void Update()
        {
            // Ambil kecepatan kamera saat ini agar proyektil tidak tertinggal saat kamera bergerak cepat
            float cameraSpeed = CameraController.Instance != null ? CameraController.Instance.CurrentSpeed : 0f;
            Vector3 cameraVelocity = Vector3.forward * cameraSpeed;

            // Hitung kecepatan total proyektil (kecepatan arah tembakan + kecepatan maju kamera)
            Vector3 projectileVelocity = (direction * speed) + cameraVelocity;

            // Pindahkan posisi proyektil
            transform.position += projectileVelocity * Time.deltaTime;
            
            // Jarak tempuh dihitung berdasarkan kecepatan tembakannya saja agar range tetap konsisten
            traveledDistance += speed * Time.deltaTime;

            if (traveledDistance >= maxRange)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            Crystal crystal = other.GetComponent<Crystal>();
            if (crystal != null && !crystal.IsDead)
            {
                // Beritahu crystal siapa yang menyerang (untuk scoring)
                crystal.TakeDamage(1, ownerPlayer);

                // Daftarkan hit ke GameManager beserta info player
                HyperSmashGameManager.Instance?.RegisterHit(ownerPlayer);

                Destroy(gameObject);
            }
        }
        #endregion
    }
}