using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class PlayerWandController : MonoBehaviour
    {
        [Header("── Player Identity ────────────────────")]
        [SerializeField] private int playerIndex = 1;

        [Header("── Wand Transform ──────────────────────")]
        [SerializeField] private Transform wandRoot;

        [Header("── Rotation Settings ───────────────────")]
        [SerializeField] private float maxTiltAngle = 45f;
        [SerializeField] private float rotateSpeed = 8f;
        [SerializeField] private float returnSpeed = 6f;

        [Header("── Gyro Settings ───────────────────────")]
        [SerializeField] private bool useGyro = false;
        [SerializeField] private float gyroSensitivity = 0.003f;

        [Header("── Tip Light ────────────────────────────")]
        [SerializeField] private Light tipLight;
        [SerializeField] private Color p1Color = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color p2Color = new Color(1f, 0.4f, 0.2f);

        private Vector2 inputDir = Vector2.zero;
        private Quaternion targetRot = Quaternion.identity;

        void Start()
        {
            if (tipLight != null) tipLight.color = playerIndex == 1 ? p1Color : p2Color;
        }

        void Update()
        {
            if (!useGyro) ReadKeyboard();
            ApplyRotation();
        }

        private void ReadKeyboard()
        {
            float h = 0f, v = 0f;
            if (playerIndex == 1)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) h = -1f; else if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
                if (Input.GetKey(KeyCode.UpArrow)) v = 1f; else if (Input.GetKey(KeyCode.DownArrow)) v = -1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.A)) h = -1f; else if (Input.GetKey(KeyCode.D)) h = 1f;
                if (Input.GetKey(KeyCode.W)) v = 1f; else if (Input.GetKey(KeyCode.S)) v = -1f;
            }
            inputDir = new Vector2(h, v);
        }

        private void ApplyRotation()
        {
            if (wandRoot == null) return;

            float rotX, rotZ;

            // Logika baru untuk mengambil Euler Angles dari WandSerialReader
            if (useGyro && playerIndex == 1 && WandSerialReader.Instance != null && WandSerialReader.Instance.IsConnected)
            {
                Vector3 euler = WandSerialReader.Instance.EulerAngles;

                // Gunakan euler.y (yaw) dan euler.x (pitch) tergantung pada sumbu mana ESP32 kamu disejajarkan
                rotX = Mathf.Clamp(-euler.y * gyroSensitivity, -maxTiltAngle, maxTiltAngle);
                rotZ = Mathf.Clamp(-euler.x * gyroSensitivity, -maxTiltAngle, maxTiltAngle);

                targetRot = Quaternion.Euler(rotX, 0f, rotZ);
            }
            else
            {
                bool hasInput = inputDir.sqrMagnitude > 0.01f;
                if (hasInput)
                {
                    rotX = inputDir.y * maxTiltAngle;
                    rotZ = -inputDir.x * maxTiltAngle;
                    targetRot = Quaternion.Euler(rotX, 0f, rotZ);
                    wandRoot.localRotation = Quaternion.Lerp(wandRoot.localRotation, targetRot, Time.deltaTime * rotateSpeed);
                }
                else
                {
                    wandRoot.localRotation = Quaternion.Lerp(wandRoot.localRotation, Quaternion.identity, Time.deltaTime * returnSpeed);
                }
                return;
            }

            wandRoot.localRotation = Quaternion.Lerp(wandRoot.localRotation, targetRot, Time.deltaTime * rotateSpeed);
        }
    }
}