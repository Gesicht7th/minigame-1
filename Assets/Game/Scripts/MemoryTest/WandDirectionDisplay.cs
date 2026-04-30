// Assets/_Game/Scripts/MemoryTest/WandDirectionDisplay.cs
// Merotasi model wand 3D di scene untuk menunjukkan arah target

using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class WandDirectionDisplay : MonoBehaviour
    {
        [Header("── Wand Parts ──────────────────────────")]
        [SerializeField] private Transform wandRoot;      // Root seluruh wand
        [SerializeField] private Renderer tipRenderer;  // Renderer ujung wand

        [Header("── Rotation per Arah ──────────────────")]
        // Rotasi wand untuk tiap arah — sesuaikan dengan model kamu
        [SerializeField] private Vector3 rotUp = new Vector3(-40f, 0f, 0f);
        [SerializeField] private Vector3 rotDown = new Vector3(40f, 0f, 0f);
        [SerializeField] private Vector3 rotLeft = new Vector3(0f, 0f, 40f);
        [SerializeField] private Vector3 rotRight = new Vector3(0f, 0f, -40f);
        [SerializeField] private Vector3 rotIdle = new Vector3(0f, 0f, 0f);

        [Header("── Colors ──────────────────────────────")]
        [SerializeField] private Color colorUp = new Color(0.1f, 0.5f, 1.0f);
        [SerializeField] private Color colorDown = new Color(1.0f, 0.4f, 0.1f);
        [SerializeField] private Color colorLeft = new Color(0.8f, 0.1f, 1.0f);
        [SerializeField] private Color colorRight = new Color(0.1f, 1.0f, 0.4f);
        [SerializeField] private Color colorIdle = new Color(0.5f, 0.2f, 0.8f);

        [Header("── Animation ───────────────────────────")]
        [SerializeField] private float rotateSpeed = 8f;
        [SerializeField] private float floatAmplitude = 0.08f;
        [SerializeField] private float floatSpeed = 1.5f;

        [Header("── Lights ──────────────────────────────")]
        [SerializeField] private Light tipLight;

        // Private
        private Quaternion targetRotation;
        private Vector3 basePosition;
        private float floatTimer;
        private bool isShowing;
        private Material tipMat;

        void Start()
        {
            basePosition = wandRoot != null ? wandRoot.localPosition : Vector3.zero;
            targetRotation = Quaternion.Euler(rotIdle);
            if (tipRenderer != null) tipMat = tipRenderer.material;
        }

        void Update()
        {
            if (wandRoot == null) return;

            // 1. Smooth rotation (Sudah benar)
            wandRoot.localRotation = Quaternion.Lerp(
                wandRoot.localRotation, targetRotation,
                Time.deltaTime * rotateSpeed);

            // 2. Logic Melayang (Floating Animation) - TAMBAHKAN INI
            floatTimer += Time.deltaTime * floatSpeed;
            float newY = basePosition.y + (Mathf.Sin(floatTimer) * floatAmplitude);

            // Terapkan posisi baru (melayang di sumbu Y lokal)
            wandRoot.localPosition = new Vector3(basePosition.x, newY, basePosition.z);
        }

        public void ShowDirection(WandDirection dir)
        {
            isShowing = true;
            Vector3 rot = GetRotation(dir);
            Color color = GetColor(dir);

            targetRotation = Quaternion.Euler(rot);
            ApplyColor(color, 2.5f);
        }

        public void HideDirection()
        {
            isShowing = false;
            targetRotation = Quaternion.Euler(rotIdle);
            ApplyColor(colorIdle, 1f);
        }

        private void ApplyColor(Color c, float intensity)
        {
            if (tipMat != null)
            {
                tipMat.color = c;
                if (tipMat.HasProperty("_EmissionColor"))
                {
                    tipMat.EnableKeyword("_EMISSION");
                    tipMat.SetColor("_EmissionColor", c * intensity);
                }
            }
            if (tipLight != null)
            {
                tipLight.color = c;
                tipLight.intensity = intensity;
            }
        }

        private Vector3 GetRotation(WandDirection dir) => dir switch
        {
            WandDirection.Up => rotUp,
            WandDirection.Down => rotDown,
            WandDirection.Left => rotLeft,
            WandDirection.Right => rotRight,
            _ => rotIdle
        };

        private Color GetColor(WandDirection dir) => dir switch
        {
            WandDirection.Up => colorUp,
            WandDirection.Down => colorDown,
            WandDirection.Left => colorLeft,
            WandDirection.Right => colorRight,
            _ => colorIdle
        };
    }
}