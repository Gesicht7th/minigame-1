// Assets/_Game/Scripts/HyperSmash/ShootingSystem.cs
// ─────────────────────────────────────────────────────────────
// PERUBAHAN DARI VERSI SEBELUMNYA:
//   - Singleton dihapus → static array Instances[2]
//   - Tambah field PlayerIndex playerIndex
//   - Fire() mengambil AimRay dari WandAimController yang sesuai
//   - Projectile diberi tahu playerIndex siapa yang menembak
//   - RegisterShot dikirim ke GameManager beserta playerIndex
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class ShootingSystem : MonoBehaviour
    {
        #region Static Access (menggantikan Singleton tunggal)
        /// <summary>
        /// Instances[0] = Player1, Instances[1] = Player2
        /// </summary>
        public static ShootingSystem[] Instances { get; private set; } = new ShootingSystem[2];

        /// <summary>Helper: ambil instance by enum</summary>
        public static ShootingSystem Get(PlayerIndex idx) => Instances[(int)idx];
        #endregion

        #region Inspector
        [Header("── Player Identity ──────────────────────────")]
        [Tooltip("Tentukan ini milik Player1 atau Player2")]
        [SerializeField] private PlayerIndex playerIndex = PlayerIndex.Player1;

        [Header("── Config ──────────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;

        [Header("── Spawn Point ─────────────────────────────")]
        [Tooltip("Dari mana projectile ditembak. Default: posisi kamera")]
        [SerializeField] private Transform firePoint;

        [Header("── Projectile Visual ───────────────────────")]
        [Tooltip("Prefab projectile. Jika null, dibuat secara procedural")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("── Projectile Color ──────────────────────────")]
        [Tooltip("Warna projectile Player1 (default: gold)")]
        [SerializeField] private Color player1Color = new Color(1f, 0.8f, 0.0f);
        [Tooltip("Warna projectile Player2 (default: cyan)")]
        [SerializeField] private Color player2Color = new Color(0.0f, 0.8f, 1f);
        #endregion

        #region Private
        private bool isShooting = false;
        private float nextFireTime = 0f;
        private float fireInterval;
        #endregion

        #region Public Properties
        public PlayerIndex PlayerIdx => playerIndex;
        public int TotalShots { get; private set; } = 0;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            int idx = (int)playerIndex;
            if (Instances[idx] != null && Instances[idx] != this)
            {
                Destroy(gameObject);
                return;
            }
            Instances[idx] = this;

            fireInterval = 1f / config.fireRate;
        }

        void Start()
        {
            if (firePoint == null && Camera.main != null)
                firePoint = Camera.main.transform;
        }

        void Update()
        {
            if (!isShooting) return;

            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireInterval;
            }
        }

        void OnDestroy()
        {
            int idx = (int)playerIndex;
            if (Instances[idx] == this)
                Instances[idx] = null;
        }
        #endregion

        #region Shooting
        private void Fire()
        {
            // Ambil WandAimController sesuai player ini
            WandAimController aimCtrl = WandAimController.Get(playerIndex);
            if (aimCtrl == null) return;

            Ray aimRay = aimCtrl.AimRay;

            // Cari titik jatuh tembakan (target) sejauh max range
            Vector3 targetPoint = aimRay.GetPoint(config.projectileRange);

            // Tentukan posisi awal
            Vector3 spawnPos = firePoint != null ? firePoint.position : aimRay.origin;

            // Arahkan proyektil dari titik spawn (Wand/Kamera) menuju target point (Crosshair)
            Vector3 fireDirection = (targetPoint - spawnPos).normalized;

            // Buat projectile
            GameObject projObj = CreateProjectile();
            projObj.transform.position = spawnPos + fireDirection * 0.5f;

            Projectile proj = projObj.GetComponent<Projectile>()
                              ?? projObj.AddComponent<Projectile>();

            // Inisialisasi projectile + info siapa yang menembak
            proj.Initialize(fireDirection, config.projectileSpeed, config.projectileRange, playerIndex);

            TotalShots++;

            // Daftarkan tembakan ke GameManager (dengan info playerIndex)
            HyperSmashGameManager.Instance?.RegisterShot(playerIndex);
        }

        private GameObject CreateProjectile()
        {
            if (projectilePrefab != null)
                return Instantiate(projectilePrefab);

            // Buat secara procedural
            GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            proj.name = $"Projectile_P{(int)playerIndex + 1}";
            proj.transform.localScale = Vector3.one * 0.15f;

            // Warna berbeda per player
            Color projColor = (playerIndex == PlayerIndex.Player1) ? player1Color : player2Color;

            Renderer rend = proj.GetComponent<Renderer>();
            Material mat = new Material(
                Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")
            );
            mat.color = projColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", projColor * 2f);
            }
            rend.material = mat;

            var col = proj.GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.15f;

            var rb = proj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            return proj;
        }
        #endregion

        #region Control
        public void StartShooting()
        {
            isShooting = true;
            // Beri sedikit jeda berbeda agar tembakan P1 & P2 tidak sinkron persis
            float initialDelay = (playerIndex == PlayerIndex.Player1) ? 0.5f : 0.75f;
            nextFireTime = Time.time + initialDelay;
            TotalShots = 0;
        }

        public void StopShooting()
        {
            isShooting = false;
        }
        #endregion
    }
}