// Assets/_Game/Scripts/HyperSmash/Crystal.cs
// ─────────────────────────────────────────────────────────────
// PERUBAHAN DARI VERSI SEBELUMNYA:
//   - TakeDamage() sekarang menerima parameter PlayerIndex killer
//   - Die() meneruskan info killer ke HyperSmashGameManager
//   - OnCrystalDestroyed event membawa info killer player
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class Crystal : MonoBehaviour
    {
        #region Inspector
        [Header("── Type & Stats ──────────────────────────")]
        [SerializeField] private CrystalType crystalType = CrystalType.Blue;

        [Tooltip("Jumlah tembakan (hit) yang dibutuhkan untuk hancur")]
        [SerializeField] private int customHP = 1;

        [Tooltip("Skor yang didapat. Isi minus (contoh: -50) untuk Bomb.")]
        [SerializeField] private int customScore = 10;

        [Header("── Visual References ─────────────────────")]
        [SerializeField] private Renderer crystalRenderer;
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private ParticleSystem breakParticles;

        [Header("── Animation ───────────────────────────────")]
        [SerializeField] private float rotateSpeed = 45f;
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float floatRange = 0.2f;

        [Header("── Destroy ─────────────────────────────────")]
        [SerializeField] private float lifetimeIfNotHit = 15f;
        #endregion

        #region Private
        private CrystalData data;
        private int currentHP;
        private bool isDead = false;
        private Vector3 startLocalPos;
        private float floatTime;
        private Material mat;

        // Siapa yang pertama kali menyerang kristal ini
        private PlayerIndex lastAttacker = PlayerIndex.Player1;
        #endregion

        #region Events
        /// <summary>Event: (crystal, killerPlayer)</summary>
        public event System.Action<Crystal, PlayerIndex> OnCrystalDestroyed;
        #endregion

        #region Public Properties
        public CrystalType Type => crystalType;
        public int ScoreValue => customScore;
        public bool IsDead => isDead;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            data = CrystalData.GetData(crystalType);
            currentHP = customHP;

            if (crystalRenderer != null)
                mat = crystalRenderer.material;

            startLocalPos = transform.localPosition;
            floatTime = Random.Range(0f, Mathf.PI * 2f);
        }

        void Start()
        {
            // Jika kristal ini diletakkan manual di dalam blok (EndlessRoomLoop), jangan hancurkan otomatis!
            if (transform.GetComponentInParent<EndlessRoomLoop>() != null) return;
            
            Destroy(gameObject, lifetimeIfNotHit);
        }

        void Update()
        {
            if (isDead) return;

            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);

            floatTime += Time.deltaTime * floatSpeed;
            float yOffset = Mathf.Sin(floatTime) * floatRange;
            transform.localPosition = startLocalPos + Vector3.up * yOffset;
        }
        #endregion

        #region Hit & Damage
        /// <summary>
        /// Dipanggil saat projectile mengenai kristal ini.
        /// killer = player yang menembak projectile tersebut.
        /// </summary>
        public void TakeDamage(int damage, PlayerIndex killer)
        {
            if (isDead) return;

            lastAttacker = killer;
            currentHP -= damage;

            StartCoroutine(HitFlash());
            if (hitParticles != null)
                hitParticles.Play();

            if (currentHP <= 0)
                Die(killer);
        }

        /// <summary>Overload tanpa killer (default Player1) — kompatibilitas.</summary>
        public void TakeDamage(int damage = 1)
        {
            TakeDamage(damage, PlayerIndex.Player1);
        }

        private void Die(PlayerIndex killer)
        {
            if (isDead) return;
            isDead = true;

            if (breakParticles != null)
            {
                breakParticles.transform.SetParent(null);
                breakParticles.Play();
                Destroy(breakParticles.gameObject, breakParticles.main.duration + 1f);
            }

            // Notify score system — sertakan siapa yang membunuh
            OnCrystalDestroyed?.Invoke(this, killer);
            HyperSmashGameManager.Instance?.OnCrystalKilled(this, killer);

            // Jika statis di dalam goa, sembunyikan saja agar bisa di-respawn nanti
            if (transform.GetComponentInParent<EndlessRoomLoop>() != null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject, 0.05f);
            }
        }

        public void Respawn()
        {
            isDead = false;
            currentHP = customHP;
            gameObject.SetActive(true);
            if (mat != null) mat.color = Color.white; // Reset warna jika sisa HitFlash
            
            // Reset rotasi & posisi lokal jika diperlukan
            floatTime = Random.Range(0f, Mathf.PI * 2f);
        }

        private IEnumerator HitFlash()
        {
            if (mat == null) yield break;

            Color originalColor = mat.color;
            mat.color = Color.white;
            yield return new WaitForSeconds(0.06f);

            if (!isDead)
            {
                float hpRatio = (float)currentHP / customHP;
                mat.color = Color.Lerp(Color.red, originalColor, hpRatio);
            }
        }
        #endregion

        #region Setup
        public void Initialize(CrystalType type)
        {
            crystalType = type;
            data = CrystalData.GetData(type);
            currentHP = customHP;
            ApplyTypeVisual();
        }

        private void ApplyTypeVisual()
        {
            float scale = crystalType switch
            {
                CrystalType.Blue => 0.7f,
                CrystalType.Purple => 1.0f,
                CrystalType.Red => 1.4f,
                CrystalType.Bomb => 0.9f,
                _ => 1f
            };
            transform.localScale = Vector3.one * scale;
        }
        #endregion
    }
}