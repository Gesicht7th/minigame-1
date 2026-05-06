// Assets/_Game/Scripts/Input/DirectionDetector.cs
// Mengonversi data raw IMU → WandDirection enum

using System;
using UnityEngine;

namespace WizardPunk
{
    public class DirectionDetector : MonoBehaviour
    {
        #region Singleton
        public static DirectionDetector Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("── Detection Settings ─────────────────")]
        [Tooltip("Threshold tilt accelerometer (raw unit). Tuning setelah kalibrasi.")]
        [SerializeField] private float tiltThreshold = 6000f;

        [Tooltip("Dead zone dalam threshold. Mencegah flicker di area threshold.")]
        [SerializeField] private float deadZone = 1000f;

        [Header("── Smoothing ────────────────────────────")]
        [Tooltip("Jumlah sample untuk averaging. Lebih tinggi = lebih smooth tapi lebih lambat.")]
        [Range(1, 20)]
        [SerializeField] private int smoothingSamples = 8;

        [Header("── Keyboard Fallback ────────────────────")]
        [Tooltip("Aktifkan keyboard untuk testing tanpa hardware")]
        [SerializeField] private bool enableKeyboardFallback = true;

        [Header("── Debug ──────────────────────────────────")]
        [SerializeField] private bool showDebugGUI = false;
        #endregion

        #region Public Properties
        public WandDirection CurrentDirection { get; private set; } = WandDirection.None;
        public float RawAX { get; private set; }
        public float RawAY { get; private set; }
        public float SmoothedAX { get; private set; }
        public float SmoothedAY { get; private set; }
        #endregion

        #region Events
        public event Action<WandDirection> OnDirectionChanged;
        #endregion

        #region Private Fields
        private float[] axBuffer;
        private float[] ayBuffer;
        private int bufferIndex = 0;
        private int sampleCount = 0;
        private WandDirection previousDirection = WandDirection.None;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            axBuffer = new float[smoothingSamples];
            ayBuffer = new float[smoothingSamples];
        }

        void Start()
        {
            if (SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived += OnSerialData;
        }

        void Update()
        {
            // Keyboard fallback: update CurrentDirection tiap frame
            if (enableKeyboardFallback)
            {
                UpdateKeyboardInput();
            }

            // Fire event jika arah berubah
            if (CurrentDirection != previousDirection)
            {
                OnDirectionChanged?.Invoke(CurrentDirection);
                previousDirection = CurrentDirection;
            }
        }

        void OnDestroy()
        {
            if (SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived -= OnSerialData;
        }
        #endregion

        #region Serial Data Processing
        private void OnSerialData(WandInputData data)
        {
            RawAX = data.ax;
            RawAY = data.ay;

            // Simpan ke rolling buffer
            axBuffer[bufferIndex] = data.ax;
            ayBuffer[bufferIndex] = data.ay;
            bufferIndex = (bufferIndex + 1) % smoothingSamples;
            sampleCount = Mathf.Min(sampleCount + 1, smoothingSamples);

            // Hitung rata-rata
            float sumAx = 0, sumAy = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                sumAx += axBuffer[i];
                sumAy += ayBuffer[i];
            }
            SmoothedAX = sumAx / sampleCount;
            SmoothedAY = sumAy / sampleCount;

            // Hanya update dari serial jika keyboard tidak aktif
            if (!enableKeyboardFallback || !HasKeyboardInput())
            {
                CurrentDirection = ClassifyDirection(SmoothedAX, SmoothedAY);
            }
        }

        private WandDirection ClassifyDirection(float ax, float ay)
        {
            float absAx = Mathf.Abs(ax);
            float absAy = Mathf.Abs(ay);
            float effectiveThreshold = tiltThreshold;

            // Hysteresis: gunakan threshold lebih rendah saat sudah dalam kondisi tilt
            if (CurrentDirection != WandDirection.None)
                effectiveThreshold = tiltThreshold - deadZone;

            // Tidak ada tilt signifikan
            if (absAx < effectiveThreshold && absAy < effectiveThreshold)
                return WandDirection.None;

            // Tentukan sumbu dominan
            if (absAy >= absAx)
            {
                // ─── Orientasi wand: tip mengarah ke atas saat netral ───
                // ay negatif = wand ditilt ke ATAS (tip ke arah langit-langit)
                // ay positif = wand ditilt ke BAWAH (tip ke lantai)
                // SESUAIKAN dengan orientasi fisik MPU6050 di wand kamu!
                return ay < 0 ? WandDirection.Up : WandDirection.Down;
            }
            else
            {
                // ax positif = tilt ke KANAN
                // ax negatif = tilt ke KIRI
                return ax > 0 ? WandDirection.Right : WandDirection.Left;
            }
        }
        #endregion

        #region Keyboard Fallback
        private bool HasKeyboardInput()
        {
            return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                   Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
        }

        private void UpdateKeyboardInput()
        {
            if (Input.GetKey(KeyCode.UpArrow))
                CurrentDirection = WandDirection.Up;
            else if (Input.GetKey(KeyCode.DownArrow))
                CurrentDirection = WandDirection.Down;
            else if (Input.GetKey(KeyCode.LeftArrow))
                CurrentDirection = WandDirection.Left;
            else if (Input.GetKey(KeyCode.RightArrow))
                CurrentDirection = WandDirection.Right;
            else if (!SerialManager.Instance?.IsConnected ?? true)
                CurrentDirection = WandDirection.None; // Keyboard only: reset saat key dilepas
        }

        /// <summary>
        /// Dipanggil RoundController untuk mendapatkan input saat ini
        /// </summary>
        public WandDirection PollDirection()
        {
            return CurrentDirection;
        }
        #endregion

        #region Debug GUI
        void OnGUI()
        {
            if (!showDebugGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 250, 160));
            GUILayout.Box("── Wand Debug ──");
            GUILayout.Label($"Serial: {(SerialManager.Instance?.IsConnected == true ? "✅ Connected" : "❌ Disconnected")}");
            GUILayout.Label($"Raw AX: {RawAX:F0} | AY: {RawAY:F0}");
            GUILayout.Label($"Smooth AX: {SmoothedAX:F0} | AY: {SmoothedAY:F0}");
            GUILayout.Label($"Direction: <b>{CurrentDirection}</b>");
            GUILayout.Label($"Threshold: {tiltThreshold}");
            GUILayout.EndArea();
        }
        #endregion
    }
}