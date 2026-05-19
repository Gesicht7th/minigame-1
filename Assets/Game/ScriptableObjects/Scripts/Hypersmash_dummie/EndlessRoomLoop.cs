using UnityEngine;

public class EndlessRoomLoop : MonoBehaviour
{
    [Header("Referensi Blok Ruangan")]
    [Tooltip("Masukkan GameObject 1, 2, dan 3 secara berurutan ke sini")]
    public Transform[] roomBlocks;

    [Header("Pengaturan Posisi")]
    [Tooltip("Panjang/Ukuran dari setiap blok ruangan pada Sumbu Z")]
    public float blockLength = 100f;

    [Tooltip("Target yang bergerak (masukkan Main Camera ke sini)")]
    public Transform playerCamera;

    [Tooltip("Jarak toleransi sebelum blok dipindah. Set 0 agar saat masuk blok 2, blok 1 langsung pindah ke depan (susunan jadi 2,3,1)")]
    public float safeZone = 0f; 

    private float spawnZ = 0f;
    private int oldestBlockIndex = 0; // Melacak blok mana yang posisinya paling belakang

    void Start()
    {
        // 1. Susun posisi awal ketiga blok secara berurutan (Z = 0, Z = Panjang, Z = Panjang * 2)
        for (int i = 0; i < roomBlocks.Length; i++)
        {
            if (roomBlocks[i] != null)
            {
                // Tetap pertahankan posisi X dan Y, hanya ubah posisi Z
                roomBlocks[i].position = new Vector3(roomBlocks[i].position.x, roomBlocks[i].position.y, spawnZ);
                spawnZ += blockLength;
            }
        }
    }

    void Update()
    {
        if (playerCamera == null || roomBlocks.Length == 0) return;

        // 2. Ambil referensi blok yang posisinya paling belakang saat ini
        Transform oldestBlock = roomBlocks[oldestBlockIndex];

        // 3. Logika Endless (Sesuai Permintaan Anda):
        // Jika kamera sudah melewati ujung blok paling belakang persis
        // (Contoh: Player masuk ke Blok 2, maka Blok 1 langsung dipindah)
        if (playerCamera.position.z >= (oldestBlock.position.z + blockLength + safeZone))
        {
            // 4. Pindahkan blok 1 ke urutan paling depan
            oldestBlock.position = new Vector3(oldestBlock.position.x, oldestBlock.position.y, spawnZ);
            
            // 4b. RESPAWN SEMUA KRISTAL DI BLOK INI (Agar yang sudah hancur muncul lagi)
            WizardPunk.HyperSmash.Crystal[] crystals = oldestBlock.GetComponentsInChildren<WizardPunk.HyperSmash.Crystal>(true);
            foreach(WizardPunk.HyperSmash.Crystal c in crystals)
            {
                c.Respawn();
            }

            // 5. Tambahkan nilai Z untuk tempat blok selanjutnya nanti
            spawnZ += blockLength;

            // 6. Update giliran indeks blok terlama (akan berputar 0 -> 1 -> 2 -> 0 -> 1 -> dst)
            oldestBlockIndex = (oldestBlockIndex + 1) % roomBlocks.Length;
        }
    }
}
