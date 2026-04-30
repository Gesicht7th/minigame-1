// Assets/_Game/Scripts/HyperSmash/WandAimController.cs
// ─────────────────────────────────────────────────────────────
// PERUBAHAN DARI VERSI SEBELUMNYA:
//   - Singleton dihapus → diganti static array Instances[2]
//   - Tambah field PlayerIndex playerIndex
//   - Player1 aim  : Mouse
//   - Player2 aim  : WASD (W=Atas, S=Bawah, A=Kiri, D=Kanan)
//   - Gyro serial tetap bisa dipakai untuk Player1
// ─────────────────────────────────────────────────────────────

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class WandAimController : MonoBehaviour
    {
        #region Static Access (menggantikan Singleton tunggal)
        /// <summary>
        /// Instances[0] = Player1, Instances[1] = Player2
        /// </summary>
        public static WandAimController[] Instances { get; private set; } = new WandAimController[2];

        /// <summary>Helper: ambil instance by enum</summary>
        public static WandAimController Get(PlayerIndex idx) => Instances[(int)idx];
        #endregion

        #region Inspector
        [Header("── Player Identity ──────────────────────────")]
        [Tooltip("Tentukan ini Player1 atau Player2")]
        [SerializeField] private PlayerIndex playerIndex = PlayerIndex.Player1;

        [Header("── Configuration ────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;

        [Header("── Input Override ───────────────────────────")]
        [Tooltip("Player1: Mouse. Player2: WASD. Centang ini untuk pakai mouse (hanya P1).")]
        [SerializeField] private bool useMouseFallback = true;
        [SerializeField] private float mouseSensitivity = 0.5f;

        [Tooltip("Kecepatan gerak aim keyboard (Player2 / WASD)")]
        [SerializeField] private float keyboardAimSpeed = 0.6f;

        [Header("── Debug ──────────────────────────────────────")]
        [SerializeField] private bool showDebugGUI = true;
        #endregion

        #region Public Properties
        public PlayerIndex PlayerIdx => playerIndex;

        /// <summary>Posisi crosshair di screen space (0,0=kiri bawah, 1,1=kanan atas)</summary>
        public Vector2 CrosshairNormalized { get; private set; } = new Vector2(0.5f, 0.5f);

        /// <summary>Posisi crosshair dalam pixel</summary>
        public Vector2 CrosshairScreenPos => new Vector2(
            CrosshairNormalized.x * Screen.width,
            CrosshairNormalized.y * Screen.height
        );

        /// <summary>Ray dari kamera ke arah crosshair — dipakai ShootingSystem</summary>
        public Ray AimRay => Camera.main.ScreenPointToRay(CrosshairScreenPos);
        #endregion

        #region Private
        private float gyroOffsetX = 0f;
        private float gyroOffsetY = 0f;
        private Vector2 targetNormalized = new Vector2(0.5f, 0.5f);
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            int idx = (int)playerIndex;
            if (Instances[idx] != null && Instances[idx] != this)
            {
                Destroy(gameObject);
                return;
            }
            Instances[idx] = this;
        }

        void Start()
        {
            // Hanya Player1 yang subscribe ke serial gyro
            if (playerIndex == PlayerIndex.Player1 && SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived += OnSerialData;

            // Lock cursor hanya jika Player1 pakai mouse
            if (playerIndex == PlayerIndex.Player1 && useMouseFallback)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            // Player1 → Mouse atau Gyro serial
            if (playerIndex == PlayerIndex.Player1 && useMouseFallback)
                UpdateMouseFallback();

            // Player2 → WASD keyboard
            if (playerIndex == PlayerIndex.Player2)
                UpdateKeyboardWASD();

            // Smooth menuju target
            CrosshairNormalized = Vector2.Lerp(
                CrosshairNormalized,
                targetNormalized,
                Time.deltaTime * config.aimSmoothing
            );
        }

        void OnDestroy()
        {
            if (playerIndex == PlayerIndex.Player1 && SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived -= OnSerialData;

            int idx = (int)playerIndex;
            if (Instances[idx] == this)
                Instances[idx] = null;
        }
        #endregion

        #region Gyro Processing (Player1 only)
        private void OnSerialData(WandInputData data)
        {
            float deltaX = data.gy * config.aimSensitivity;
            float deltaY = data.gx * config.aimSensitivity;

            gyroOffsetX += deltaX;
            gyroOffsetY -= deltaY;

            float border = config.aimBorderLimit;
            gyroOffsetX = Mathf.Clamp(gyroOffsetX, -0.5f + border, 0.5f - border);
            gyroOffsetY = Mathf.Clamp(gyroOffsetY, -0.5f + border, 0.5f - border);

            targetNormalized = new Vector2(0.5f + gyroOffsetX, 0.5f + gyroOffsetY);
        }
        #endregion

        #region Mouse Fallback (Player1)
        private void UpdateMouseFallback()
        {
            bool serialConnected = SerialManager.Instance?.IsConnected ?? false;
            if (!serialConnected)
            {
                float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                float border = config.aimBorderLimit;
                targetNormalized.x = Mathf.Clamp(targetNormalized.x + mx, border, 1f - border);
                targetNormalized.y = Mathf.Clamp(targetNormalized.y + my, border, 1f - border);
            }
        }
        #endregion

        #region WASD Keyboard Aim (Player2)
        private void UpdateKeyboardWASD()
        {
            float h = 0f, v = 0f;

            if (Input.GetKey(KeyCode.A)) h = -1f;
            else if (Input.GetKey(KeyCode.D)) h = 1f;

            if (Input.GetKey(KeyCode.S)) v = -1f;
            else if (Input.GetKey(KeyCode.W)) v = 1f;

            float border = config.aimBorderLimit;
            float speed = keyboardAimSpeed * Time.deltaTime;

            targetNormalized.x = Mathf.Clamp(targetNormalized.x + h * speed, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(targetNormalized.y + v * speed, border, 1f - border);
        }
        #endregion

        #region Public Methods
        public void ResetToCenter()
        {
            gyroOffsetX = 0f;
            gyroOffsetY = 0f;
            targetNormalized = new Vector2(0.5f, 0.5f);
            CrosshairNormalized = new Vector2(0.5f, 0.5f);
        }
        #endregion

        #region Debug GUI
        void OnGUI()
        {
            if (!showDebugGUI) return;

            // Pisahkan posisi debug box agar tidak tumpang tindih
            float yOffset = (playerIndex == PlayerIndex.Player1) ? 170f : 290f;
            string label = playerIndex == PlayerIndex.Player1 ? "P1" : "P2";

            GUILayout.BeginArea(new Rect(10, yOffset, 230, 100));
            GUILayout.Box($"── Aim Debug [{label}] ──");
            GUILayout.Label($"Normalized: {CrosshairNormalized.x:F2}, {CrosshairNormalized.y:F2}");
            GUILayout.Label($"Screen Px: {CrosshairScreenPos.x:F0}, {CrosshairScreenPos.y:F0}");
            GUILayout.Label($"Input: {(playerIndex == PlayerIndex.Player1 ? "Mouse/Gyro" : "WASD")}");
            GUILayout.EndArea();
        }
        #endregion
    }
}