using UnityEngine;

// Assets/_Game/Scripts/Core/GameState.cs
// Definisi semua enum yang dipakai di seluruh game

namespace WizardPunk
{
    /// <summary>
    /// State machine utama GameManager
    /// </summary>
    public enum GameState
    {
        Idle,           // Di main menu, belum main
        Countdown,      // Hitung mundur 3-2-1-GO
        Playing,        // Game berjalan
        RoundFeedback,  // Menampilkan feedback benar/salah
        GameOver        // Game selesai
    }

    /// <summary>
    /// Arah tongkat yang dideteksi
    /// </summary>
    public enum WandDirection
    {
        None,   // Tidak ada tilt / netral
        Up,     // Tongkat dimiringkan ke atas
        Down,   // Tongkat dimiringkan ke bawah
        Left,   // Tongkat dimiringkan ke kiri
        Right   // Tongkat dimiringkan ke kanan
    }

    /// <summary>
    /// Hasil evaluasi satu ronde
    /// </summary>
    public enum RoundResult
    {
        Correct,    // Input benar
        Wrong,      // Input salah
        Timeout     // Tidak ada input
    }
}
