// Assets/_Game/Scripts/UI/CursorGlobal.cs
//
// Global Cursor Controller
//
// Fungsi:
// - Hide system cursor Unity
// - Control virtual cursor visibility
// - Singleton global access
// - Aman dari conflict UnityEngine.Cursor
//
// Cara Pakai:
//
// Hide cursor:
// CursorGlobal.HideVirtualCursor();
//
// Show cursor:
// CursorGlobal.ShowVirtualCursor();
//
// Register virtual cursor:
// otomatis lewat Inspector
//

using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class CursorGlobal : MonoBehaviour
    {
        // ============================================================
        // SINGLETON
        // ============================================================

        public static CursorGlobal Instance { get; private set; }

        // ============================================================
        // INSPECTOR
        // ============================================================

        [Header("── Virtual Cursor ─────────────────────")]
        [SerializeField] private Image virtualCursor;

        [Header("── Cursor Settings ─────────────────────")]
        [SerializeField] private bool hideSystemCursor = true;

        [SerializeField]
        private CursorLockMode lockMode =
            CursorLockMode.Locked;

        // ============================================================
        // STATE
        // ============================================================

        private bool isVisible = false;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Hide system cursor
            ApplySystemCursorSettings();

            // Hide virtual cursor at start
            HideVirtualCursor();
        }

        private void OnDisable()
        {
            // Restore system cursor
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState =
                CursorLockMode.None;
        }

        // ============================================================
        // SYSTEM CURSOR
        // ============================================================

        private void ApplySystemCursorSettings()
        {
            if (hideSystemCursor)
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = lockMode;
            }
        }

        // ============================================================
        // STATIC API
        // ============================================================

        public static void ShowVirtualCursor()
        {
            if (Instance == null) return;

            Instance.ShowCursor_Internal();
        }

        public static void HideVirtualCursor()
        {
            if (Instance == null) return;

            Instance.HideCursor_Internal();
        }

        public static bool IsCursorVisible()
        {
            if (Instance == null) return false;

            return Instance.isVisible;
        }

        // ============================================================
        // INTERNAL
        // ============================================================

        private void ShowCursor_Internal()
        {
            isVisible = true;

            ApplySystemCursorSettings();

            if (virtualCursor != null)
            {
                virtualCursor.enabled = true;
            }
        }

        private void HideCursor_Internal()
        {
            isVisible = false;

            ApplySystemCursorSettings();

            if (virtualCursor != null)
            {
                virtualCursor.enabled = false;
            }
        }

        // ============================================================
        // OPTIONAL REGISTER
        // ============================================================

        public void RegisterCursor(Image cursorImage)
        {
            virtualCursor = cursorImage;
        }
    }
}