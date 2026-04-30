// Assets/_Game/Scripts/Visual3D/Wand3DVisual.cs
// Merotasi model 3D wand di scene sesuai data gyro real-time

using UnityEngine;

namespace WizardPunk
{
    public class Wand3DVisual : MonoBehaviour
    {
        #region Inspector Fields
        [Header("── Rotation Mapping ────────────────────")]
        [Tooltip("Sensitivitas rotasi. Sesuaikan agar visual terasa natural.")]
        [SerializeField] private float rotationSensitivity = 0.003f;

        [Tooltip("Maximum angle rotasi dari posisi netral (derajat)")]
        [SerializeField] private float maxRotationAngle = 45f;

        [Tooltip("Smooth factor rotasi. Lebih tinggi = lebih smooth.")]
        [Range(1f, 20f)]
        [SerializeField] private float smoothSpeed = 8f;

        [Header("── Idle Animation ───────────────────────")]
        [SerializeField] private bool enableIdleFloat = true;
        [SerializeField] private float floatAmplitude = 0.05f;
        [SerializeField] private float floatSpeed = 1.5f;

        [Header("── Particle Tips ────────────────────────")]
        [SerializeField] private ParticleSystem tipParticles;
        [SerializeField] private Light tipLight;
        [SerializeField] private float correctLightIntensity = 3f;
        [SerializeField] private float normalLightIntensity = 1f;

        [Header("── Colors ───────────────────────────────")]
        [SerializeField] private Color correctColor = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color wrongColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color normalColor = new Color(0.5f, 0.2f, 1f);
        #endregion

        #region Private Fields
        private Quaternion targetRotation;
        private Quaternion baseRotation;
        private Vector3 basePosition;
        private float floatTime = 0f;
        private bool isReactingToInput = false;
        #endregion

        #region Unity Lifecycle
        void Start()
        {
            baseRotation = transform.rotation;
            basePosition = transform.position;
            targetRotation = baseRotation;

            if (SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived += OnSerialData;

            if (tipLight != null)
                tipLight.color = normalColor;
        }

        void Update()
        {
            // Smooth rotation
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * smoothSpeed
            );

            // Idle float animation
            if (enableIdleFloat && !isReactingToInput)
            {
                floatTime += Time.deltaTime * floatSpeed;
                float yOffset = Mathf.Sin(floatTime) * floatAmplitude;
                transform.position = basePosition + Vector3.up * yOffset;
            }
        }

        void OnDestroy()
        {
            if (SerialManager.Instance != null)
                SerialManager.Instance.OnDataReceived -= OnSerialData;
        }
        #endregion

        #region Serial Data
        private void OnSerialData(WandInputData data)
        {
            // Konversi data IMU ke rotasi 3D
            // ax → rotasi sumbu Z (kiri-kanan)
            // ay → rotasi sumbu X (depan-belakang)

            float rotX = Mathf.Clamp(-data.ay * rotationSensitivity, -maxRotationAngle, maxRotationAngle);
            float rotZ = Mathf.Clamp(-data.ax * rotationSensitivity, -maxRotationAngle, maxRotationAngle);

            targetRotation = baseRotation * Quaternion.Euler(rotX, 0f, rotZ);
        }
        #endregion

        #region Visual Feedback
        public void PlayCorrectEffect()
        {
            isReactingToInput = true;
            StartCoroutine(FlashEffect(correctColor, 0.5f));
        }

        public void PlayWrongEffect()
        {
            isReactingToInput = true;
            StartCoroutine(FlashEffect(wrongColor, 0.5f));
        }

        private System.Collections.IEnumerator FlashEffect(Color flashColor, float duration)
        {
            if (tipLight != null)
            {
                tipLight.color = flashColor;
                tipLight.intensity = correctLightIntensity;
            }

            if (tipParticles != null)
            {
                var main = tipParticles.main;
                main.startColor = flashColor;
                tipParticles.Play();
            }

            yield return new UnityEngine.WaitForSeconds(duration);

            if (tipLight != null)
            {
                tipLight.color = normalColor;
                tipLight.intensity = normalLightIntensity;
            }

            if (tipParticles != null)
                tipParticles.Stop();

            isReactingToInput = false;
        }
        #endregion
    }
}