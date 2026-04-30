// Assets/_Game/Scripts/HyperSmash/CameraController.cs

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// Menggerakkan Main Camera maju otomatis (sumbu +Z).
    /// Kecepatan dikontrol DifficultyManager.
    /// Attach ke: Main Camera atau parent-nya.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region Singleton
        public static CameraController Instance { get; private set; }
        #endregion

        #region Inspector
        [Header("── Config ──────────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;

        [Header("── State ───────────────────────────────────")]
        [SerializeField] private bool isMoving = false;
        #endregion

        #region Public Properties
        public float CurrentSpeed { get; private set; }
        public float DistanceTraveled { get; private set; }
        public bool IsMoving => isMoving;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            CurrentSpeed = config.initialCameraSpeed;
        }

        void Update()
        {
            if (!isMoving) return;

            // Gerak maju
            float moveAmount = CurrentSpeed * Time.deltaTime;
            transform.Translate(Vector3.forward * moveAmount, Space.World);
            DistanceTraveled += moveAmount;

            // Tingkatkan kecepatan (capped di maxCameraSpeed)
            CurrentSpeed = Mathf.Min(
                CurrentSpeed + config.speedIncreaseRate * Time.deltaTime,
                config.maxCameraSpeed
            );
        }
        #endregion

        #region Public Methods
        public void StartMoving()
        {
            isMoving = true;
            CurrentSpeed = config.initialCameraSpeed;
            DistanceTraveled = 0f;
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        public void ResetPosition(Vector3 startPosition)
        {
            transform.position = startPosition;
            DistanceTraveled = 0f;
            CurrentSpeed = config.initialCameraSpeed;
        }
        #endregion
    }
}