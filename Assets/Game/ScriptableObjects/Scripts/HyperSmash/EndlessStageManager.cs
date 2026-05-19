// Assets/_Game/Scripts/HyperSmash/EndlessStageManager.cs

using UnityEngine;
using System.Collections.Generic;

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// Mengelola stage secara endless menggunakan sistem Object Pooling.
    /// Stage akan terus dibuat di depan player (kamera) dan dihancurkan (dikembalikan ke pool) di belakang.
    /// </summary>
    public class EndlessStageManager : MonoBehaviour
    {
        [Header("── Block Settings ────────────────────────")]
        [Tooltip("Prefabs blok stage yang akan di-spawn. Bisa masukkan 3 blok asset yang berbeda-beda.")]
        public GameObject[] blockPrefabs;
        
        [Tooltip("Panjang dari setiap blok (Z-axis). Pastikan ukurannya akurat dengan model prefab.")]
        public float blockLength = 100f;
        
        [Tooltip("Berapa blok yang selalu aktif di layar sekaligus (Idealnya 2 atau 3)")]
        public int activeBlockCount = 2;

        [Header("── References ────────────────────────────")]
        [Tooltip("Transform dari Camera (atau Player) untuk acuan deteksi posisi")]
        public Transform playerTransform;

        private List<GameObject> activeBlocks = new List<GameObject>();
        
        // Optimisasi Tingkat Lanjut: Object Pooling untuk menghindari lag/stutter saat Instantiate/Destroy
        private Dictionary<string, Queue<GameObject>> blockPools = new Dictionary<string, Queue<GameObject>>();

        private float spawnZ = 0f;
        
        private void Start()
        {
            if (playerTransform == null && Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }

            // Inisialisasi pool untuk setiap prefab
            if (blockPrefabs != null)
            {
                foreach (var prefab in blockPrefabs)
                {
                    if (prefab != null && !blockPools.ContainsKey(prefab.name))
                    {
                        blockPools[prefab.name] = new Queue<GameObject>();
                    }
                }
            }
        }

        /// <summary>
        /// Dipanggil oleh GameManager saat memulai ronde baru
        /// </summary>
        public void InitializeEndlessStage()
        {
            // Kembalikan semua blok aktif ke pool
            foreach(var block in activeBlocks) 
            {
                ReturnToPool(block);
            }
            activeBlocks.Clear();
            
            // Reset titik spawn (bisa disesuaikan jika kamera mulai dari Z tertentu)
            spawnZ = playerTransform != null ? playerTransform.position.z : 0f;

            // Spawn blok awal untuk memulai
            for (int i = 0; i < activeBlockCount; i++)
            {
                SpawnBlock();
            }
        }

        private void Update()
        {
            if (playerTransform == null || activeBlocks.Count == 0) return;

            // Logika Endless Loop:
            // Jika posisi Z player sudah melewati ujung blok pertama
            // Maka kita memunculkan blok baru di depan, dan menyimpan blok lama ke pool
            if (playerTransform.position.z > activeBlocks[0].transform.position.z + blockLength)
            {
                SpawnBlock();
                DeleteOldBlock();
            }
        }

        private void SpawnBlock()
        {
            if (blockPrefabs == null || blockPrefabs.Length == 0) return;

            // Pilih tipe blok (bisa acak atau berurutan, disini acak)
            int randomIndex = Random.Range(0, blockPrefabs.Length);
            GameObject prefabToSpawn = blockPrefabs[randomIndex];
            
            // Ambil dari Object Pool agar performa sangat optimal
            GameObject block = GetFromPool(prefabToSpawn);

            block.transform.position = new Vector3(0, 0, spawnZ);
            block.transform.rotation = Quaternion.identity;
            block.SetActive(true);
            
            activeBlocks.Add(block);
            
            // Majukan titik spawn untuk blok berikutnya
            spawnZ += blockLength;
        }

        private void DeleteOldBlock()
        {
            if (activeBlocks.Count > 0)
            {
                GameObject oldBlock = activeBlocks[0];
                ReturnToPool(oldBlock); // Masukkan kembali ke pool, BUKAN Destroy
                activeBlocks.RemoveAt(0);
            }
        }

        #region --- Sistem Object Pool ---
        
        private GameObject GetFromPool(GameObject prefab)
        {
            if (blockPools.ContainsKey(prefab.name) && blockPools[prefab.name].Count > 0)
            {
                return blockPools[prefab.name].Dequeue();
            }

            // Jika pool kosong, buat baru (biasanya hanya terjadi di awal permainan)
            GameObject newObj = Instantiate(prefab);
            newObj.name = prefab.name; // Kunci string harus sama dengan nama pool
            newObj.transform.SetParent(this.transform);
            return newObj;
        }

        private void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            if (blockPools.ContainsKey(obj.name))
            {
                blockPools[obj.name].Enqueue(obj);
            }
            else
            {
                Destroy(obj); // Fallback pengaman
            }
        }
        
        #endregion
    }
}
