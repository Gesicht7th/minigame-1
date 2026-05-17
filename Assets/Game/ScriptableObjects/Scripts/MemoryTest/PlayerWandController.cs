using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class PlayerWandController : MonoBehaviour
    {
        [Header("── Player Identity ────────────────────")]
        [SerializeField] private int playerIndex = 1;

        [Header("── Wand Serial Reader ─────────────────")]
        [SerializeField] private WandSerialReader serialReader;

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
            // Auto-find sederhana jika belum diassign
            if (serialReader == null)
            {
                WandSerialReader[] readers = FindObjectsByType<WandSerialReader>(FindObjectsSortMode.None);

                foreach (var reader in readers)
                {
                    string n = reader.name.ToUpper();

                    if (playerIndex == 1 &&
                        (n.Contains("P1") || n.Contains("PLAYER1")))
                    {
                        serialReader = reader;
                    }

                    if (playerIndex == 2 &&
                        (n.Contains("P2") || n.Contains("PLAYER2")))
                    {
                        serialReader = reader;
                    }
                }

                // fallback
                if (serialReader == null && readers.Length > 0)
                {
                    serialReader = (playerIndex == 1)
                        ? readers[0]
                        : (readers.Length > 1 ? readers[1] : readers[0]);
                }
            }

            if (tipLight != null)
                tipLight.color = playerIndex == 1 ? p1Color : p2Color;

            Debug.Log($"[PlayerWandController] P{playerIndex} linked to: {(serialReader != null ? serialReader.name : "NONE")}");
        }

        void Update()
        {
            if (!useGyro)
                ReadKeyboard();

            ApplyRotation();
        }

        private void ReadKeyboard()
        {
            float h = 0f;
            float v = 0f;

            if (playerIndex == 1)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
                else if (Input.GetKey(KeyCode.RightArrow)) h = 1f;

                if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
                else if (Input.GetKey(KeyCode.DownArrow)) v = -1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.A)) h = -1f;
                else if (Input.GetKey(KeyCode.D)) h = 1f;

                if (Input.GetKey(KeyCode.W)) v = 1f;
                else if (Input.GetKey(KeyCode.S)) v = -1f;
            }

            inputDir = new Vector2(h, v);
        }

        private void ApplyRotation()
        {
            if (wandRoot == null) return;

            float rotX;
            float rotZ;

            // ===== GYRO MODE =====
            if (useGyro &&
                serialReader != null &&
                serialReader.IsConnected)
            {
                Vector3 euler = serialReader.EulerAngles;

                rotX = Mathf.Clamp(
                    -euler.y * gyroSensitivity,
                    -maxTiltAngle,
                    maxTiltAngle);

                rotZ = Mathf.Clamp(
                    -euler.x * gyroSensitivity,
                    -maxTiltAngle,
                    maxTiltAngle);

                targetRot = Quaternion.Euler(rotX, 0f, rotZ);

                wandRoot.localRotation = Quaternion.Lerp(
                    wandRoot.localRotation,
                    targetRot,
                    Time.deltaTime * rotateSpeed);

                return;
            }

            // ===== KEYBOARD FALLBACK =====
            bool hasInput = inputDir.sqrMagnitude > 0.01f;

            if (hasInput)
            {
                rotX = inputDir.y * maxTiltAngle;
                rotZ = -inputDir.x * maxTiltAngle;

                targetRot = Quaternion.Euler(rotX, 0f, rotZ);

                wandRoot.localRotation = Quaternion.Lerp(
                    wandRoot.localRotation,
                    targetRot,
                    Time.deltaTime * rotateSpeed);
            }
            else
            {
                wandRoot.localRotation = Quaternion.Lerp(
                    wandRoot.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * returnSpeed);
            }
        }
    }
}