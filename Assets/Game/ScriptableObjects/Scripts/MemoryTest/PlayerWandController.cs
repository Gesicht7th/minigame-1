using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class PlayerWandController : MonoBehaviour
    {
        [Header("── Player Identity ────────────────────")]
        [Tooltip("1 = Arrow Keys | 2 = WASD")]
        [SerializeField] private int playerIndex = 1;

        [Header("── Wand Transform ──────────────────────")]
        [Tooltip("Transform wand yang akan dirotasi (WandRoot)")]
        [SerializeField] private Transform wandRoot;

        [Header("── Rotation Settings ───────────────────")]
        [Tooltip("Sudut maksimum wand saat ditekan (derajat)")]
        [SerializeField] private float maxTiltAngle = 45f;
        [Tooltip("Kecepatan rotasi wand")]
        [SerializeField] private float rotateSpeed = 8f;
        [Tooltip("Kecepatan kembali ke netral saat tidak ada input")]
        [SerializeField] private float returnSpeed = 6f;

        [Header("── Gyro Settings ───────────────────────")]
        [Tooltip("Aktifkan gyro ESP32 (matikan saat pakai keyboard)")]
        [SerializeField] private bool useGyro = false;
        [Tooltip("Sensitivitas gyro ke rotasi wand")]
        [SerializeField] private float gyroSensitivity = 0.003f;

        [Header("── Tip Light ────────────────────────────")]
        [SerializeField] private Light tipLight;
        [SerializeField] private Color p1Color = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color p2Color = new Color(1f, 0.4f, 0.2f);

        // ── Private ───────────────────────────────────
        private Vector2 inputDir = Vector2.zero;
        private Quaternion targetRot = Quaternion.identity;
        private WandInputData latestData;

        // ─────────────────────────────────────────────
        void Start()
        {
            // Set warna tip light sesuai player
            if (tipLight != null)
                tipLight.color = playerIndex == 1 ? p1Color : p2Color;

            // Subscribe serial — P1 pakai port 1, P2 pakai port 2
            if (useGyro)
            {
                if (DualSerialManager.Instance != null)
                {
                    if (playerIndex == 1)
                        DualSerialManager.Instance.OnP1DataReceived += OnGyroData;
                    else
                        DualSerialManager.Instance.OnP2DataReceived += OnGyroData;
                }
                else if (SerialManager.Instance != null && playerIndex == 1)
                {
                    SerialManager.Instance.OnDataReceived += OnSingleGyroData;
                }
            }
        }

        void Update()
        {
            if (!useGyro)
                ReadKeyboard();

            ApplyRotation();
        }

        void OnDestroy()
        {
            if (!useGyro) return;
            if (DualSerialManager.Instance != null)
            {
                if (playerIndex == 1)
                    DualSerialManager.Instance.OnP1DataReceived -= OnGyroData;
                else
                    DualSerialManager.Instance.OnP2DataReceived -= OnGyroData;
            }
            if (SerialManager.Instance != null && playerIndex == 1)
                SerialManager.Instance.OnDataReceived -= OnSingleGyroData;
        }

        // ── Keyboard Input ────────────────────────────
        private void ReadKeyboard()
        {
            if (playerIndex == 1)
            {
                // Player 1: Arrow Keys
                float h = 0f, v = 0f;
                if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
                if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
                if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
                if (Input.GetKey(KeyCode.DownArrow)) v = -1f;
                inputDir = new Vector2(h, v);
            }
            else
            {
                // Player 2: WASD
                float h = 0f, v = 0f;
                if (Input.GetKey(KeyCode.A)) h = -1f;
                if (Input.GetKey(KeyCode.D)) h = 1f;
                if (Input.GetKey(KeyCode.W)) v = 1f;
                if (Input.GetKey(KeyCode.S)) v = -1f;
                inputDir = new Vector2(h, v);
            }
        }

        // ── Gyro Input ────────────────────────────────
        private void OnGyroData(WandInputData data) => latestData = data;
        private void OnSingleGyroData(WandInputData d) => latestData = d;

        // ── Apply Rotation ────────────────────────────
        private void ApplyRotation()
        {
            if (wandRoot == null) return;

            float rotX, rotZ;

            if (useGyro)
            {
                // Gyro: AY = kiri/kanan, AX = atas/bawah
                rotX = Mathf.Clamp(-latestData.ay * gyroSensitivity,
                                   -maxTiltAngle, maxTiltAngle);
                rotZ = Mathf.Clamp(-latestData.ax * gyroSensitivity,
                                   -maxTiltAngle, maxTiltAngle);
                targetRot = Quaternion.Euler(rotX, 0f, rotZ);
            }
            else
            {
                bool hasInput = inputDir.sqrMagnitude > 0.01f;

                if (hasInput)
                {
                    // Input → rotasi sesuai arah
                    rotX = inputDir.y * maxTiltAngle; // atas/bawah
                    rotZ = -inputDir.x * maxTiltAngle; // kiri/kanan
                    targetRot = Quaternion.Euler(rotX, 0f, rotZ);

                    wandRoot.localRotation = Quaternion.Lerp(
                        wandRoot.localRotation, targetRot,
                        Time.deltaTime * rotateSpeed);
                }
                else
                {
                    // Tidak ada input → kembali netral
                    wandRoot.localRotation = Quaternion.Lerp(
                        wandRoot.localRotation, Quaternion.identity,
                        Time.deltaTime * returnSpeed);
                }
                return;
            }

            wandRoot.localRotation = Quaternion.Lerp(
                wandRoot.localRotation, targetRot,
                Time.deltaTime * rotateSpeed);
        }
    }
}