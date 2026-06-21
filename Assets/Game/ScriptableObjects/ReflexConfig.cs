// Assets/_Game/Scripts/ReflexShowdown/ReflexConfig.cs

using System.Collections.Generic;
using UnityEngine;

namespace WizardPunk.Reflex
{
    [System.Serializable]
    public class AnimationDelayConfig
    {
        [Tooltip("Aksi yang dipilih Player 1 (Rock=Block, Paper=Parry, Scissors=Attack)")]
        public RpsType p1Action = RpsType.None;

        [Tooltip("Aksi yang dipilih Player 2 (Rock=Block, Paper=Parry, Scissors=Attack)")]
        public RpsType p2Action = RpsType.None;

        [Tooltip("Delay (detik) sebelum animasi Player 1 diputar")]
        [Min(0f)]
        public float p1Delay = 0f;

        [Tooltip("Delay (detik) sebelum animasi Player 2 diputar")]
        [Min(0f)]
        public float p2Delay = 0f;
    }

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
        public int gameWinBonus = 50;

        [Header("── Animation Transition Delays ─────────────")]
        [Tooltip("Atur delay animasi per kombinasi aksi P1 & P2.")]
        public List<AnimationDelayConfig> animationDelays = new List<AnimationDelayConfig>();

        public AnimationDelayConfig FindDelay(RpsType p1Action, RpsType p2Action)
        {
            if (animationDelays == null) return null;
            foreach (var entry in animationDelays)
            {
                if (entry.p1Action == p1Action && entry.p2Action == p2Action)
                    return entry;
            }
            return null;
        }
    }
}