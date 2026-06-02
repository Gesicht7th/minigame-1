using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class WandAimController : MonoBehaviour
    {
        public static WandAimController[] Instances { get; private set; } = new WandAimController[2];
        public static WandAimController Get(PlayerIndex idx) => Instances[(int)idx];

        [Header("── Player Identity ──────────────────────────")]
        [SerializeField] private PlayerIndex playerIndex = PlayerIndex.Player1;

        [Header("── Configuration ────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;

        [Header("── MPU6050 Gyro Settings (New) ──────────────")]
        public WandSerialReader serialReader;
        [Tooltip("Deadzone untuk mengabaikan getaran kecil tangan")]
        public float gyroDeadzone = 2.5f;
        [Tooltip("Sensitivitas aim (gunakan nilai kecil karena rentang layar 0-1)")]
        public float gyroSensitivity = 0.05f;

        [Header("── Input Override ───────────────────────────")]
        [SerializeField] private bool useMouseFallback = true;
        [SerializeField] private float mouseSensitivity = 0.5f;
        [SerializeField] private float keyboardAimSpeed = 0.6f;

        [Header("── Debug ──────────────────────────────────────")]
        [SerializeField] private bool showDebugGUI = true;

        public PlayerIndex PlayerIdx => playerIndex;
        public Vector2 CrosshairNormalized { get; private set; } = new Vector2(0.5f, 0.5f);
        public Vector2 CrosshairScreenPos => new Vector2(CrosshairNormalized.x * Screen.width, CrosshairNormalized.y * Screen.height);
        public Ray AimRay => Camera.main.ScreenPointToRay(CrosshairScreenPos);

        private Vector2 targetNormalized = new Vector2(0.5f, 0.5f);
        private float _debugLogTimer = 0f;

        void Awake()
        {
            int idx = (int)playerIndex;
            if (Instances[idx] != null && Instances[idx] != this) { Destroy(gameObject); return; }
            Instances[idx] = this;
        }

        void Start()
        {
            if (serialReader == null)
            {
                string targetPort = (playerIndex == PlayerIndex.Player1) ? "COM8" : "COM9";
                serialReader = WandSerialReader.GetByPort(targetPort);
                if (serialReader != null) Debug.Log($"[ReaderResolve] SUCCESS {targetPort}");
                else Debug.Log($"[ReaderResolve] FAILED {targetPort}");
            }

            if (playerIndex == PlayerIndex.Player1 && useMouseFallback && serialReader == null)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            if (playerIndex == PlayerIndex.Player1)
            {
                if (serialReader != null)
                {
                    UpdateGyroAiming();
                }
                else if (useMouseFallback)
                {
                    UpdateMouseFallback();
                }
            }

            // SESUDAH
            if (playerIndex == PlayerIndex.Player2)
            {
                if (serialReader != null)
                    UpdateGyroAiming();
                else
                    UpdateKeyboardWASD();
            }

            CrosshairNormalized = Vector2.Lerp(
                CrosshairNormalized,
                targetNormalized,
                Time.deltaTime * config.aimSmoothing
            );
        }

        void OnDestroy()
        {
            int idx = (int)playerIndex;
            if (Instances[idx] == this) Instances[idx] = null;
        }

        private float ApplyDeadzone(float rawInput, float deadZone)
        {
            if (rawInput > deadZone) return rawInput - deadZone;
            if (rawInput < -deadZone) return rawInput + deadZone;
            return 0f;
        }

        private void UpdateGyroAiming()
        {
            Vector3 gyro = serialReader.GyroVelocity;

            _debugLogTimer += Time.deltaTime;
            if (_debugLogTimer >= 1f)
            {
                _debugLogTimer = 0f;
                Debug.Log($"[{playerIndex}] Gyro={gyro} Target={targetNormalized} Crosshair={CrosshairNormalized}");
            }

            // Mapping dipertahankan dari penemuan sebelumnya: z = horizontal, x = vertical
            float inputX = ApplyDeadzone(gyro.z, gyroDeadzone);
            float inputY = ApplyDeadzone(gyro.x, gyroDeadzone);

            float deltaX = inputX * -gyroSensitivity * Time.deltaTime;
            float deltaY = inputY * gyroSensitivity * Time.deltaTime;

            float border = config.aimBorderLimit;
            targetNormalized.x = Mathf.Clamp(targetNormalized.x + deltaX, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(targetNormalized.y + deltaY, border, 1f - border);

            // Resenter crosshair jika drift terlalu jauh dan tombol fisik ditekan
            if (serialReader.ConsumeZeroed())
            {
                ResetToCenter();
            }
        }

        private void UpdateMouseFallback()
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            float border = config.aimBorderLimit;
            targetNormalized.x = Mathf.Clamp(targetNormalized.x + mx, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(targetNormalized.y + my, border, 1f - border);
        }

        private void UpdateKeyboardWASD()
        {
            float h = 0f, v = 0f;
            if (Input.GetKey(KeyCode.A)) h = -1f; else if (Input.GetKey(KeyCode.D)) h = 1f;
            if (Input.GetKey(KeyCode.S)) v = -1f; else if (Input.GetKey(KeyCode.W)) v = 1f;

            float border = config.aimBorderLimit;
            float speed = keyboardAimSpeed * Time.deltaTime;

            targetNormalized.x = Mathf.Clamp(targetNormalized.x + h * speed, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(targetNormalized.y + v * speed, border, 1f - border);
        }

        public void ResetToCenter()
        {
            targetNormalized = new Vector2(0.5f, 0.5f);
            CrosshairNormalized = new Vector2(0.5f, 0.5f);
        }

        void OnGUI()
        {
            if (!showDebugGUI) return;
            float yOffset = (playerIndex == PlayerIndex.Player1) ? 170f : 290f;
            string label = playerIndex == PlayerIndex.Player1 ? "P1" : "P2";

            GUILayout.BeginArea(new Rect(10, yOffset, 230, 100));
            GUILayout.Box($"── Aim Debug [{label}] ──");
            GUILayout.Label($"Normalized: {CrosshairNormalized.x:F2}, {CrosshairNormalized.y:F2}");
            GUILayout.Label($"Screen Px: {CrosshairScreenPos.x:F0}, {CrosshairScreenPos.y:F0}");
            GUILayout.Label($"Input: {(serialReader != null ? "MPU6050" : (useMouseFallback ? "Mouse" : "None"))}");
            GUILayout.EndArea();
        }
    }
}