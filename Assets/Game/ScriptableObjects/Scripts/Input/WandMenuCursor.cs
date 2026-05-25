// Assets/_Game/Scripts/UI/Main Menu/WandMenuCursor.cs
// Menggerakkan cursor menu menggunakan gyro velocity dari satu wand controller.
// Sistem click menggunakan action button (B:ACTION).
// Taruh di GameObject kosong di Main Menu scene.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizardPunk
{
    public class WandMenuCursor : MonoBehaviour
    {
        [Header("── Wand Input ─────────────────────────")]
        [Tooltip("WandSerialReader yang akan jadi air mouse (biasanya P1)")]
        [SerializeField] private WandSerialReader wandReader;

        [Header("── Cursor Visual ────────────────────────")]
        [Tooltip("RectTransform dari Image yang jadi cursor wand")]
        [SerializeField] private RectTransform cursorRect;

        [Header("── UI Raycasting ────────────────────────")]
        [Tooltip("GraphicRaycaster dari Canvas utama menu")]
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        [Header("── Gyro Settings ────────────────────────")]
        [Tooltip("Deadzone — abaikan getaran kecil tangan")]
        [SerializeField] private float gyroDeadzone = 2.5f;
        [Tooltip("Sensitivitas pergerakan cursor (sama seperti WandAimController)")]
        [SerializeField] private float gyroSensitivity = 0.05f;
        [Tooltip("Seberapa smooth pergerakan cursor (lerp speed)")]
        [SerializeField] private float aimSmoothing = 12f;
        [Tooltip("Batas pinggir layar supaya cursor tidak keluar")]
        [SerializeField] private float borderPadding = 0.03f;

        [Header("── Fallback ─────────────────────────────")]
        [Tooltip("Aktifkan mouse biasa jika wand tidak terhubung")]
        [SerializeField] private bool mouseFallback = true;

        // ── State ────────────────────────────────────────────────
        private Vector2 _normalized = new Vector2(0.5f, 0.5f);
        private Vector2 _targetNorm = new Vector2(0.5f, 0.5f);
        private GameObject _hovered = null;
        private bool _wandMode = false;

        // ============================================================
        void Start()
        {
            if (wandReader == null)
                wandReader = FindObjectOfType<WandSerialReader>();

            if (graphicRaycaster == null)
                graphicRaycaster = FindObjectOfType<GraphicRaycaster>();

            // Tentukan mode awal
            _wandMode = wandReader != null && wandReader.IsConnected;
            ApplyCursorMode(_wandMode);
        }

        void Update()
        {
            // Cek perubahan koneksi wand
            bool connected = wandReader != null && wandReader.IsConnected;
            if (connected != _wandMode)
            {
                _wandMode = connected;
                ApplyCursorMode(_wandMode);
            }

            if (_wandMode)
            {
                UpdateWandPosition();
            }
            else if (mouseFallback)
            {
                UpdateMouseFallback();
            }

            // Smooth position
            _normalized = Vector2.Lerp(_normalized, _targetNorm, Time.deltaTime * aimSmoothing);

            // Update visual
            UpdateCursorRect();

            // Hover + click
            HandleUIInteraction();
        }

        // ============================================================
        private void ApplyCursorMode(bool useWand)
        {
            Cursor.visible = !useWand;
            Cursor.lockState = useWand ? CursorLockMode.Locked : CursorLockMode.None;

            if (cursorRect != null)
                cursorRect.gameObject.SetActive(useWand);
        }

        // ============================================================
        private float Deadzone(float value)
        {
            if (value > gyroDeadzone) return value - gyroDeadzone;
            if (value < -gyroDeadzone) return value + gyroDeadzone;
            return 0f;
        }

        private void UpdateWandPosition()
        {
            Vector3 gyro = wandReader.GyroVelocity;

            // Axis mapping identik dengan WandAimController di HyperSmash:
            // gyro.z = horizontal, gyro.x = vertical
            float inputX = Deadzone(gyro.z);
            float inputY = Deadzone(gyro.x);

            float deltaX = inputX * -gyroSensitivity * Time.deltaTime;
            float deltaY = inputY * gyroSensitivity * Time.deltaTime;

            float b = borderPadding;
            _targetNorm.x = Mathf.Clamp(_targetNorm.x + deltaX, b, 1f - b);
            _targetNorm.y = Mathf.Clamp(_targetNorm.y + deltaY, b, 1f - b);

            // Reset ke tengah saat tombol reset fisik ditekan
            if (wandReader.ConsumeZeroed())
                RecenterCursor();
        }

        private void UpdateMouseFallback()
        {
            // Ikuti posisi mouse biasa (normalisasi dari screen)
            float b = borderPadding;
            _targetNorm.x = Mathf.Clamp(Input.mousePosition.x / Screen.width, b, 1f - b);
            _targetNorm.y = Mathf.Clamp(Input.mousePosition.y / Screen.height, b, 1f - b);
        }

        private void UpdateCursorRect()
        {
            if (cursorRect == null) return;

            // Konversi normalized → screen pixel
            cursorRect.position = new Vector3(
                _normalized.x * Screen.width,
                _normalized.y * Screen.height,
                0f
            );
        }

        // ============================================================
        private void HandleUIInteraction()
        {
            if (graphicRaycaster == null) return;

            Vector2 screenPos = new Vector2(
                _normalized.x * Screen.width,
                _normalized.y * Screen.height
            );

            // Build pointer data di posisi cursor
            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            // Raycast ke semua elemen UI
            var results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointer, results);

            // Ambil elemen paling atas yang punya Selectable (Button, dll.)
            GameObject hit = null;
            foreach (var result in results)
            {
                GameObject candidate = FindSelectable(result.gameObject);
                if (candidate != null) { hit = candidate; break; }
            }

            // ── Hover Enter / Exit ────────────────────────────────
            if (hit != _hovered)
            {
                if (_hovered != null)
                    ExecuteEvents.Execute(_hovered, pointer, ExecuteEvents.pointerExitHandler);

                if (hit != null)
                    ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerEnterHandler);

                _hovered = hit;
            }

            // ── Click (satu kali saat action button ditekan) ──────
            if (_wandMode && wandReader.ConsumeAction())
            {
                if (_hovered != null)
                {
                    ExecuteEvents.Execute(_hovered, pointer, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(_hovered, pointer, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(_hovered, pointer, ExecuteEvents.pointerClickHandler);
                    Debug.Log($"[WandCursor] Clicked: {_hovered.name}");
                }
            }
            // Fallback: klik kiri mouse
            else if (!_wandMode && mouseFallback && Input.GetMouseButtonDown(0))
            {
                if (_hovered != null)
                {
                    ExecuteEvents.Execute(_hovered, pointer, ExecuteEvents.pointerClickHandler);
                }
            }
        }

        // ── Naik hierarki sampai ketemu Selectable (Button, Toggle, dll.)
        private GameObject FindSelectable(GameObject obj)
        {
            Transform t = obj.transform;
            while (t != null)
            {
                if (t.GetComponent<Selectable>() != null) return t.gameObject;
                t = t.parent;
            }
            return null;
        }

        public void RecenterCursor()
        {
            _targetNorm = new Vector2(0.5f, 0.5f);
            _normalized = new Vector2(0.5f, 0.5f);
        }

        void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}