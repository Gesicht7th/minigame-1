// Assets/_Game/Scripts/ReflexShowdown/ReflexConfig.cs

using UnityEngine;

namespace WizardPunk.Reflex
{
    [CreateAssetMenu(
        fileName = "ReflexConfig",
        menuName = "WizardPunk/Reflex Config",
        order = 3
    )]
    public class ReflexConfig : ScriptableObject
    {
        [Header("── Round System ───────────────────────────")]
        [Tooltip("Jumlah ronde (ganjil agar ada pemenang)")]
        public int roundsPerGame = 5;
        [Tooltip("Jeda antara ronde (detik)")]
        public float interRoundDelay = 2.5f;

        [Header("── Ready Phase ────────────────────────────")]
        [Tooltip("Durasi fase ready — kedua player harus tahan wand rendah")]
        public float readyPhaseDuration = 2f;
        [Tooltip("Threshold gyro untuk 'wand rendah' (AY raw unit)")]
        public float wandLowThreshold = 4000f;
        [Tooltip("Threshold gyro untuk 'wand naik/fire position'")]
        public float wandRaisedThreshold = 8000f;

        [Header("── Countdown ──────────────────────────────")]
        public int countdownStart = 3;
        public float countdownStepDuration = 1f;
        [Tooltip("Variasi random delay sebelum GO (biar tidak predictable)")]
        public float minGoDelay = 0.5f;
        public float maxGoDelay = 2.0f;

        [Header("── Draw Phase ──────────────────────────────")]
        [Tooltip("Batas waktu reaksi — jika tidak ada yang fire, dianggap draw")]
        public float reactionTimeout = 3f;

        [Header("── False Start ─────────────────────────────")]
        [Tooltip("Jika player angkat wand sebelum GO = false start")]
        public bool falseStartPenalty = true;
        [Tooltip("Penalty waktu tambah (detik) untuk false start")]
        public float falseStartPenaltyTime = 2f;

        [Header("── Score ───────────────────────────────────")]
        [Tooltip("Poin per ronde menang")]
        public int pointsPerRoundWin = 10;
        [Tooltip("Poin bonus jika menang keseluruhan game")]
        public int gameWinBonus = 20;
    }
}