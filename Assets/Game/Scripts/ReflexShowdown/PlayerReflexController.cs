// Assets/_Game/Scripts/ReflexShowdown/PlayerReflexController.cs
// Menangani input satu player: deteksi posisi wand (rendah/naik) + fire button

using System;
using UnityEngine;

namespace WizardPunk.Reflex
{
    public enum WandPosition { Low, Raised }

    public class PlayerReflexController : MonoBehaviour
    {
        [Header("── Player Identity ────────────────────")]
        [SerializeField] private int playerIndex = 1; // 1 atau 2

        [Header("── Config ──────────────────────────────")]
        [SerializeField] private ReflexConfig config;

        [Header("── Keyboard Fallback ───────────────────")]
        [Tooltip("Player 1: [S]=hold low, [Space]=fire. Player 2: [K]=hold low, [Enter]=fire")]
        [SerializeField] private bool useKeyboard = true;
        [SerializeField] private KeyCode holdLowKey = KeyCode.S;       // P1: S | P2: K
        [SerializeField] private KeyCode fireKey = KeyCode.Space;   // P1: Space | P2: Return

        // ── State ────────────────────────────────────
        public WandPosition CurrentPosition { get; private set; } = WandPosition.Low;
        public bool FiredThisRound { get; private set; } = false;
        public float FireTimestamp { get; private set; } = -1f;
        public bool FalseStartTriggered { get; private set; } = false;

        // Events
        public event Action OnFired;
        public event Action OnFalseStart;
        public event Action<WandPosition> OnPositionChanged;

        // Internal
        private WandPosition prevPosition = WandPosition.Low;
        private bool acceptingInput = false;
        private bool inCountdown = false;

        // ─────────────────────────────────────────────
        void Start()
        {
            // Subscribe ke serial data sesuai player index
            if (DualSerialManager.Instance != null)
            {
                if (playerIndex == 1)
                    DualSerialManager.Instance.OnP1DataReceived += OnSerialData;
                else
                    DualSerialManager.Instance.OnP2DataReceived += OnSerialData;
            }
        }

        void Update()
        {
            if (useKeyboard) UpdateKeyboard();

            // Deteksi perubahan posisi
            if (CurrentPosition != prevPosition)
            {
                OnPositionChanged?.Invoke(CurrentPosition);

                // False start: angkat wand saat countdown
                if (inCountdown && CurrentPosition == WandPosition.Raised &&
                    config.falseStartPenalty && !FalseStartTriggered)
                {
                    FalseStartTriggered = true;
                    OnFalseStart?.Invoke();
                    Debug.Log($"[P{playerIndex}] FALSE START!");
                }

                prevPosition = CurrentPosition;
            }
        }

        void OnDestroy()
        {
            if (DualSerialManager.Instance == null) return;
            if (playerIndex == 1)
                DualSerialManager.Instance.OnP1DataReceived -= OnSerialData;
            else
                DualSerialManager.Instance.OnP2DataReceived -= OnSerialData;
        }

        // ── Keyboard Input ────────────────────────────
        private void UpdateKeyboard()
        {
            bool holdingLow = Input.GetKey(holdLowKey);
            CurrentPosition = holdingLow ? WandPosition.Low : WandPosition.Raised;

            if (acceptingInput && !FiredThisRound && Input.GetKeyDown(fireKey))
                RegisterFire();
        }

        // ── Serial / Gyro Input ───────────────────────
        private void OnSerialData(WandInputData data)
        {
            // ay negatif = wand diarahkan ke atas (naik)
            // ay positif = wand ke bawah / netral
            // Sesuaikan berdasarkan orientasi fisik wand
            float ay = data.ay;

            if (ay < -config.wandRaisedThreshold)
                CurrentPosition = WandPosition.Raised;
            else if (ay > -config.wandLowThreshold)
                CurrentPosition = WandPosition.Low;
            // Di antara threshold: tidak berubah (hysteresis)
        }

        // ── Fire ──────────────────────────────────────
        private void RegisterFire()
        {
            FiredThisRound = true;
            FireTimestamp = Time.time;
            OnFired?.Invoke();
            Debug.Log($"[P{playerIndex}] FIRED at {FireTimestamp:F4}s");
        }

        // ── Public Control ────────────────────────────
        public void SetCountdownMode(bool active)
        {
            inCountdown = active;
            FalseStartTriggered = false;
        }

        public void EnableFiring()
        {
            acceptingInput = true;
            FiredThisRound = false;
            FireTimestamp = -1f;
        }

        public void DisableFiring()
        {
            acceptingInput = false;
        }

        public void ResetForRound()
        {
            FiredThisRound = false;
            FireTimestamp = -1f;
            FalseStartTriggered = false;
            inCountdown = false;
            acceptingInput = false;
            CurrentPosition = WandPosition.Low;
        }

        public bool IsWandLow => CurrentPosition == WandPosition.Low;
    }
}