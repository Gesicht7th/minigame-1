using System;
using UnityEngine;

namespace WizardPunk.Reflex
{
    public enum WandPosition { Low, Raised }
    public enum RpsType { None, Rock, Paper, Scissors } // Tipe Attack

    public class PlayerReflexController : MonoBehaviour
    {
        [Header("── Player Identity ────────────────────")]
        [SerializeField] private int playerIndex = 1;

        [Header("── Config ──────────────────────────────")]
        [SerializeField] private ReflexConfig config;
        public WandSerialReader serialReader;

        [Header("── Input Method ────────────────────────")]
        [Tooltip("Centang untuk Player 2, buang centang untuk Player 1 (Wand)")]
        public bool useKeyboard = false;

        [Header("── Keyboard Mapping (P2) ───────────────")]
        public KeyCode holdKey = KeyCode.K;
        public KeyCode rockKey = KeyCode.A;       // Batu
        public KeyCode scissorsKey = KeyCode.S;   // Gunting
        public KeyCode paperKey = KeyCode.D;      // Kertas

        // ── State ────────────────────────────────────
        public WandPosition CurrentPosition { get; private set; } = WandPosition.Raised;
        public bool FiredThisRound { get; private set; } = false;
        public float FireTimestamp { get; private set; } = -1f;
        public bool FalseStartTriggered { get; private set; } = false;

        // Pilihan attack ronde ini
        public RpsType SelectedAttack { get; private set; } = RpsType.None;

        // Events
        public event Action OnFired;
        public event Action OnFalseStart;
        public event Action<WandPosition> OnPositionChanged;

        private WandPosition prevPosition = WandPosition.Raised;
        private bool acceptingInput = false;
        private bool inCountdown = false;

        void Start()
        {
            if (serialReader == null)
            {
                string targetPort = (playerIndex == 1) ? "COM8" : "COM9";
                serialReader = WandSerialReader.GetByPort(targetPort);
                Debug.Log($"[ReaderResolve] Player{playerIndex} -> {targetPort}");
            }
        }

        void Update()
        {
            if (!useKeyboard) UpdateWand();
            else UpdateKeyboard();

            // Deteksi perubahan posisi
            if (CurrentPosition != prevPosition)
            {
                OnPositionChanged?.Invoke(CurrentPosition);

                // False start: angkat wand/lepas hold saat countdown
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

        private void UpdateWand()
        {
            if (serialReader == null) return;

            // 1. Baca status Hold absolut, bukan dari tebak-tebakan timer
            bool isHolding = serialReader.IsHolding;

            // 2. Transisikan pose karakter
            CurrentPosition = isHolding ? WandPosition.Low : WandPosition.Raised;

            // 3. Tangkap gesture kapanpun
            string gesture = serialReader.ConsumeGesture();
            if (!string.IsNullOrEmpty(gesture))
            {
                if (gesture == "U") SelectedAttack = RpsType.Scissors;
                else if (gesture == "R") SelectedAttack = RpsType.Rock;
                else if (gesture == "L") SelectedAttack = RpsType.Paper;
            }

            // 4. Eksekusi serangan HANYA saat status berubah dari Hold -> Release
            if (!isHolding && acceptingInput && !FiredThisRound && SelectedAttack != RpsType.None)
            {
                RegisterFire();
            }
        }

        private void UpdateKeyboard()
        {
            // Keyboard hold konvensional
            bool holding = Input.GetKey(holdKey);
            CurrentPosition = holding ? WandPosition.Low : WandPosition.Raised;

            if (acceptingInput && !FiredThisRound)
            {
                if (Input.GetKeyDown(rockKey)) { SelectedAttack = RpsType.Rock; RegisterFire(); }
                else if (Input.GetKeyDown(scissorsKey)) { SelectedAttack = RpsType.Scissors; RegisterFire(); }
                else if (Input.GetKeyDown(paperKey)) { SelectedAttack = RpsType.Paper; RegisterFire(); }
            }
        }

        private void RegisterFire()
        {
            FiredThisRound = true;
            FireTimestamp = Time.time;
            OnFired?.Invoke();
            Debug.Log($"[P{playerIndex}] MENGELUARKAN {SelectedAttack} at {FireTimestamp:F4}s");
        }

        public void SetCountdownMode(bool active) { inCountdown = active; FalseStartTriggered = false; }
        public void EnableFiring() { acceptingInput = true; FiredThisRound = false; FireTimestamp = -1f; }
        public void DisableFiring() { acceptingInput = false; }
        public void ResetForRound()
        {
            FiredThisRound = false;
            FireTimestamp = -1f;
            FalseStartTriggered = false;
            inCountdown = false;
            acceptingInput = false;
            CurrentPosition = WandPosition.Raised; // Default ke raised
            SelectedAttack = RpsType.None;
        }
        public bool IsWandLow => CurrentPosition == WandPosition.Low;
    }
}