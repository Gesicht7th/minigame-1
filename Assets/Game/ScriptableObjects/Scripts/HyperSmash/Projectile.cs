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
            float moveAmount = speed * Time.deltaTime;
            transform.position += direction * moveAmount;
            traveledDistance += moveAmount;

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