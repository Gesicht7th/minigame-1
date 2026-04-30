using UnityEngine;

// Assets/_Game/Scripts/HyperSmash/HyperSmashTypes.cs
// Semua enum dan struct khusus untuk Hyper Smash

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// Tipe kristal — menentukan warna, HP, dan poin
    /// </summary>
    public enum CrystalType
    {
        Blue,   // 1 poin, 1 HP  — paling umum
        Purple, // 5 poin, 3 HP
        Red,    // 10 poin, 6 HP — paling langka
        Bomb    // -5 poin, 1 HP — hindari!
    }

    /// <summary>
    /// State machine game Hyper Smash
    /// </summary>
    public enum HyperSmashState
    {
        Idle,       // Belum mulai
        Countdown,  // 3-2-1-GO
        Playing,    // Sedang bermain
        RoundOver,     // (opsional)
        GameOver    // Waktu habis / game selesai
    }

    /// <summary>
    /// Data kristal: konfigurasi per tipe
    /// </summary>
    [System.Serializable]
    public struct CrystalData
    {
        public CrystalType type;
        public int maxHP;
        public int scoreValue; // positif atau negatif
        public float spawnWeight; // probabilitas spawn (lebih tinggi = lebih sering)

        public static CrystalData GetData(CrystalType type)
        {
            return type switch
            {
                CrystalType.Blue => new CrystalData { type = type, maxHP = 1, scoreValue = 1, spawnWeight = 60f },
                CrystalType.Purple => new CrystalData { type = type, maxHP = 3, scoreValue = 5, spawnWeight = 25f },
                CrystalType.Red => new CrystalData { type = type, maxHP = 6, scoreValue = 10, spawnWeight = 10f },
                CrystalType.Bomb => new CrystalData { type = type, maxHP = 1, scoreValue = -5, spawnWeight = 5f },
                _ => default
            };
        }
    }

    /// <summary>
    /// Data hasil game — dikirim ke Result Scene
    /// </summary>
    [System.Serializable]
    public struct HyperSmashResult
    {
        public int finalScore;
        public int blueDestroyed;
        public int purpleDestroyed;
        public int redDestroyed;
        public int bombsHit;
        public int totalShots;
        public int totalHits;

        public float Accuracy => totalShots > 0 ? (float)totalHits / totalShots * 100f : 0f;
    }
}