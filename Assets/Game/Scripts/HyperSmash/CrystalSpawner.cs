// Assets/_Game/Scripts/HyperSmash/CrystalSpawner.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// Spawner kristal dan bomb. Bekerja dengan sistem gelombang (wave).
    /// Pola kemunculan dirancang simetris agar rapi seperti Smash Hit.
    /// </summary>
    public class CrystalSpawner : MonoBehaviour
    {
        #region Singleton
        public static CrystalSpawner Instance { get; private set; }
        #endregion

        #region Inspector
        [Header("── Config ──────────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;

        [Header("── Prefabs (Opsional — bisa pakai Factory) ─")]
        [Tooltip("Jika di-assign, pakai prefab. Jika null, pakai CrystalFactory")]
        [SerializeField] private GameObject prefabBlueCrystal;
        [SerializeField] private GameObject prefabPurpleCrystal;
        [SerializeField] private GameObject prefabRedCrystal;
        [SerializeField] private GameObject prefabBomb;

        [Header("── Corridor Parent ─────────────────────────")]
        [Tooltip("Parent object untuk crystal agar hierarki rapi")]
        [SerializeField] private Transform crystalContainer;
        #endregion

        #region Private
        private bool isSpawning = false;
        private float currentSpawnInterval;
        private int totalSpawned = 0;

        // Bobot untuk weighted random tipe kristal
        private readonly float[] typeWeights = { 60f, 25f, 10f, 5f }; // Blue, Purple, Red, Bomb
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        #endregion

        #region Control
        public void StartSpawning()
        {
            isSpawning = true;
            currentSpawnInterval = config.initialSpawnInterval;
            totalSpawned = 0;
            StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            isSpawning = false;
            StopAllCoroutines();
        }
        #endregion

        #region Spawn Loop
        private IEnumerator SpawnLoop()
        {
            while (isSpawning)
            {
                SpawnWave();

                // Percepat interval seiring waktu
                currentSpawnInterval = Mathf.Max(
                    config.minSpawnInterval,
                    currentSpawnInterval - config.spawnIntervalDecay * currentSpawnInterval
                );

                yield return new WaitForSeconds(currentSpawnInterval);
            }
        }

        private void SpawnWave()
        {
            if (CameraController.Instance == null) return;

            // Hitung posisi spawn (di depan kamera)
            Vector3 cameraPos = Camera.main.transform.position;
            float spawnZ = cameraPos.z + config.spawnDistanceAhead;

            // Tentukan jumlah kristal dalam gelombang ini
            int count = Random.Range(config.minCrystalsPerWave, config.maxCrystalsPerWave + 1);

            // Pilih formasi
            FormationType formation = PickFormation();
            SpawnFormation(formation, count, spawnZ);

            totalSpawned += count;
        }
        #endregion

        #region Formations
        private enum FormationType
        {
            Random,     // Menyebar bebas (Z diatur agar tidak numpuk)
            Line,       // Sejajar horizontal rapi
            Symmetric,  // Terbagi adil ke kiri dan kanan
            Triangle    // Segitiga simetris
        }

        private FormationType PickFormation()
        {
            int rand = Random.Range(0, 4);
            return (FormationType)rand;
        }

        private void SpawnFormation(FormationType formation, int count, float spawnZ)
        {
            List<Vector3> positions = GenerateFormationPositions(formation, count, spawnZ);

            foreach (Vector3 pos in positions)
            {
                CrystalType type = PickRandomType();
                SpawnCrystalAt(type, pos);
            }
        }

        private List<Vector3> GenerateFormationPositions(FormationType formation, int count, float z)
        {
            List<Vector3> positions = new List<Vector3>();

            // Memberikan jarak aman agar kristal tidak tertelan dinding lorong
            float safeWidth = (config.corridorWidth * 0.5f) - 3f;
            float safeHeight = (config.corridorHeight * 0.5f) - 3f;

            switch (formation)
            {
                case FormationType.Line:
                case FormationType.Symmetric:
                    // LOGIKA BARU: SMART GRID 3D
                    // Jika kristalnya banyak, otomatis disusun jadi beberapa baris (Y) dan kolom (X)
                    int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
                    int rows = Mathf.CeilToInt((float)count / cols);

                    float stepX = (safeWidth * 2) / (cols + 1);
                    float stepY = (safeHeight * 2) / (rows + 1);

                    int current = 0;
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            if (current >= count) break;

                            float xPos = -safeWidth + (stepX * (c + 1));
                            // Susun dari atas ke bawah (mengisi sumbu Y)
                            float yPos = safeHeight - (stepY * (r + 1));

                            positions.Add(new Vector3(xPos, yPos, z));
                            current++;
                        }
                    }
                    break;

                case FormationType.Triangle:
                    // Formasi V terbalik (maksimal 4, sisanya jadikan acak di belakangnya)
                    positions.Add(new Vector3(0f, safeHeight * 0.5f, z)); // Puncak tengah
                    if (count >= 2) positions.Add(new Vector3(-safeWidth * 0.6f, -safeHeight * 0.4f, z)); // Kiri bawah
                    if (count >= 3) positions.Add(new Vector3(safeWidth * 0.6f, -safeHeight * 0.4f, z)); // Kanan bawah
                    if (count >= 4) positions.Add(new Vector3(0f, -safeHeight * 0.8f, z)); // Bawah tengah

                    // Jika minta lebih dari 4, sisa kristalnya disebar acak
                    for (int i = 4; i < count; i++)
                    {
                        positions.Add(new Vector3(Random.Range(-safeWidth, safeWidth), Random.Range(-safeHeight, safeHeight), z + (i * 2f)));
                    }
                    break;

                case FormationType.Random:
                default:
                    // Tersebar bebas 3D (X dan Y acak), Z dimundurkan agar tidak saling tabrak
                    for (int i = 0; i < count; i++)
                    {
                        positions.Add(new Vector3(
                            Random.Range(-safeWidth, safeWidth),
                            Random.Range(-safeHeight, safeHeight),
                            z + (i * 3f) // Jarak 3 unit ke belakang antar kristal
                        ));
                    }
                    break;
            }

            return positions;
        }
        #endregion

        #region Type Selection
        private CrystalType PickRandomType()
        {
            float total = 0f;
            foreach (float w in typeWeights) total += w;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;

            CrystalType[] types = { CrystalType.Blue, CrystalType.Purple, CrystalType.Red, CrystalType.Bomb };
            for (int i = 0; i < types.Length; i++)
            {
                cumulative += typeWeights[i];
                if (roll <= cumulative) return types[i];
            }

            return CrystalType.Blue;
        }
        #endregion

        #region Spawn
        private void SpawnCrystalAt(CrystalType type, Vector3 worldPosition)
        {
            GameObject crystalObj;

            GameObject prefab = GetPrefab(type);
            if (prefab != null)
            {
                crystalObj = Instantiate(prefab, worldPosition, Random.rotation);
            }
            else
            {
                crystalObj = CrystalFactory.CreateCrystal(type);
                crystalObj.transform.position = worldPosition;
                crystalObj.transform.rotation = Random.rotation;
            }

            if (crystalContainer != null)
                crystalObj.transform.SetParent(crystalContainer);
        }

        private GameObject GetPrefab(CrystalType type)
        {
            return type switch
            {
                CrystalType.Blue => prefabBlueCrystal,
                CrystalType.Purple => prefabPurpleCrystal,
                CrystalType.Red => prefabRedCrystal,
                CrystalType.Bomb => prefabBomb,
                _ => null
            };
        }
        #endregion
    }
}