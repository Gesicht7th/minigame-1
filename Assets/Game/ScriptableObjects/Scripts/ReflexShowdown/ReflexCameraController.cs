// Assets/_Game/Scripts/ReflexShowdown/ReflexCameraController.cs
// Kamera side-view mirip Tekken/Street Fighter

using UnityEngine;

namespace WizardPunk.Reflex
{
    public class ReflexCameraController : MonoBehaviour
    {
        [Header("── Targets ─────────────────────────────")]
        [SerializeField] private Transform player1Transform;
        [SerializeField] private Transform player2Transform;

        [Header("── Camera Settings ────────────────────")]
        [SerializeField] private float cameraHeight = 1.5f;  // Y kamera
        [SerializeField] private float cameraDistance = 7f;    // Jarak Z dari midpoint
        [SerializeField] private float followSmoothing = 3f;

        [Header("── Fixed Side View ─────────────────────")]
        [Tooltip("Aktifkan untuk kamera fixed tidak mengikuti pemain")]
        [SerializeField] private bool useFixedPosition = true;
        [SerializeField] private Vector3 fixedPosition = new Vector3(0f, 1.5f, -7f);
        [SerializeField] private Vector3 fixedRotation = new Vector3(8f, 0f, 0f);

        private Camera cam;
        private Vector3 targetPos;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (useFixedPosition)
            {
                transform.position = fixedPosition;
                transform.eulerAngles = fixedRotation;
            }
        }

        void LateUpdate()
        {
            if (useFixedPosition) return;
            if (player1Transform == null || player2Transform == null) return;

            // Midpoint antara 2 player
            Vector3 midpoint = (player1Transform.position + player2Transform.position) * 0.5f;
            targetPos = new Vector3(midpoint.x, cameraHeight, midpoint.z - cameraDistance);

            transform.position = Vector3.Lerp(transform.position, targetPos,
                                              Time.deltaTime * followSmoothing);

            // Selalu lihat ke midpoint
            transform.LookAt(new Vector3(midpoint.x, midpoint.y + 0.5f, midpoint.z));
        }
    }
}