// Assets/_Game/Scripts/HyperSmash/HyperSmashConfig.cs

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    [CreateAssetMenu(
        fileName = "HyperSmashConfig",
        menuName = "WizardPunk/HyperSmash Config",
        order = 2
    )]
    public class HyperSmashConfig : ScriptableObject
    {
        [Header("── Round System ──────────────────────────────")]
        [Tooltip("Jumlah ronde per sesi game ini (2 atau 3)")]
        public int roundsPerGame = 3;
        [Tooltip("Durasi tiap ronde (detik)")]
        public float roundDurationSeconds = 60f;
        [Tooltip("Jeda antar ronde (detik) — tampilkan layar inter-round")]
        public float interRoundBreakDuration = 3f;

        [Header("── Camera / Movement ───────────────────────")]
        [Tooltip("Kecepatan kamera maju awal (unit/detik)")]
        public float initialCameraSpeed = 8f;
        [Tooltip("Kecepatan maksimum kamera")]
        public float maxCameraSpeed = 25f;
        [Tooltip("Pertambahan kecepatan per detik")]
        public float speedIncreaseRate = 0.05f;

        [Header("── Aiming ───────────────────────────────────")]
        [Tooltip("Sensitivitas gyro ke gerakan crosshair (0-1 screen space)")]
        public float aimSensitivity = 0.00003f;
        [Tooltip("Seberapa smooth crosshair mengikuti gerakan wand")]
        [Range(1f, 20f)]
        public float aimSmoothing = 10f;
        [Tooltip("Batas crosshair dari tepi layar (0=tepi, 0.5=tengah)")]
        [Range(0.05f, 0.45f)]
        public float aimBorderLimit = 0.1f;

        [Header("── Shooting ─────────────────────────────────")]
        [Tooltip("Berapa projectile yang ditembak per detik (auto-fire)")]
        public float fireRate = 3f; // shots per second
        [Tooltip("Kecepatan projectile (unit/detik)")]
        public float projectileSpeed = 30f;
        [Tooltip("Jarak maksimum projectile sebelum destroy")]
        public float projectileRange = 50f;

        [Header("── Spawning ─────────────────────────────────")]
        [Tooltip("Jarak spawn kristal di depan kamera")]
        public float spawnDistanceAhead = 40f;
        [Tooltip("Interval antar gelombang spawn (detik)")]
        public float initialSpawnInterval = 2f;
        [Tooltip("Interval spawn minimum")]
        public float minSpawnInterval = 0.5f;
        [Tooltip("Penurunan interval spawn per detik")]
        public float spawnIntervalDecay = 0.01f;
        [Tooltip("Jumlah kristal per gelombang (min)")]
        public int minCrystalsPerWave = 2;
        [Tooltip("Jumlah kristal per gelombang (max)")]
        public int maxCrystalsPerWave = 5;

        [Header("── Corridor Layout ──────────────────────────")]
        [Tooltip("Lebar koridor (unit). Kristal di-spawn dalam range ini")]
        public float corridorWidth = 8f;
        [Tooltip("Tinggi koridor (unit)")]
        public float corridorHeight = 5f;

        [Header("── Countdown ────────────────────────────────")]
        public int countdownStart = 3;
        public float countdownStepDuration = 1f;

        [Header("── Scoring ──────────────────────────────────")]
        [Tooltip("Skor minimum (tidak bisa di bawah ini)")]
        public int minimumScore = -50;
    }
}