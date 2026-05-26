// Assets/_Game/Scripts/UI/CursorGlobal.cs
//
// GLOBAL POINTER SYSTEM
//
// Fungsi:
// - Hide real Windows cursor sepanjang gameplay
// - Show real cursor hanya saat ESC
// - Manage virtual pointer visibility
// - Reusable untuk semua minigame
// - Pointer logic tetap aktif walaupun visual hidden
//
// NOTE:
// Script ini TIDAK menggerakkan pointer.
// Script ini hanya mengatur visibility dan global state.
//
// Pointer movement tetap ditangani oleh:
// - WandPointerController
// - WandCursorController
// - atau script movement lain
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

        public static CursorGlobal Instance
        {
            get;
            private set;
        }

        // ============================================================
        // INSPECTOR
        // ============================================================

        [Header("── Virtual Pointer ───────────────────")]

        [Tooltip("Image UI pointer virtual")]
        [SerializeField]
        private Image virtualPointer;

        [Header("── System Cursor ─────────────────────")]

        [Tooltip("Hide cursor asli Windows saat game")]
        [SerializeField]
        private bool hideSystemCursor = true;

        [Tooltip("Cursor lock mode saat gameplay")]
        [SerializeField]
        private CursorLockMode gameplayLockMode =
            CursorLockMode.Locked;

        [Header("── ESC Debug ─────────────────────────")]

        [Tooltip("ESC memunculkan cursor asli")]
        [SerializeField]
        private bool allowEscapeCursor = true;

        // ============================================================
        // STATE
        // ============================================================

        private bool pointerVisible = false;

        private bool realCursorVisible = false;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            // Singleton
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // ========================================================
            // REAL CURSOR
            // ========================================================

            HideRealCursor();

            // ========================================================
            // VIRTUAL POINTER
            // ========================================================

            HideVirtualCursor();
        }

        private void Update()
        {
            // ========================================================
            // ESC = SHOW REAL CURSOR
            // ========================================================

            if (allowEscapeCursor &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                ShowRealCursor();
            }
        }

        private void OnDisable()
        {
            // Restore cursor saat object disable
            UnityEngine.Cursor.visible = true;

            UnityEngine.Cursor.lockState =
                CursorLockMode.None;
        }

        // ============================================================
        // REAL CURSOR CONTROL
        // ============================================================

        public static void HideRealCursor()
        {
            if (Instance == null) return;

            Instance.HideRealCursor_Internal();
        }

        public static void ShowRealCursor()
        {
            if (Instance == null) return;

            Instance.ShowRealCursor_Internal();
        }

        private void HideRealCursor_Internal()
        {
            realCursorVisible = false;

            if (hideSystemCursor)
            {
                UnityEngine.Cursor.visible = false;

                UnityEngine.Cursor.lockState =
                    gameplayLockMode;
            }
        }

        private void ShowRealCursor_Internal()
        {
            realCursorVisible = true;

            UnityEngine.Cursor.visible = true;

            UnityEngine.Cursor.lockState =
                CursorLockMode.None;
        }

        // ============================================================
        // VIRTUAL POINTER CONTROL
        // ============================================================

        public static void ShowVirtualCursor()
        {
            if (Instance == null) return;

            Instance.ShowVirtualCursor_Internal();
        }

        public static void HideVirtualCursor()
        {
            if (Instance == null) return;

            Instance.HideVirtualCursor_Internal();
        }

        private void ShowVirtualCursor_Internal()
        {
            pointerVisible = true;

            if (virtualPointer != null)
            {
                // IMPORTANT:
                // Jangan pakai SetActive(false)
                // supaya movement script tetap jalan

                virtualPointer.enabled = true;
            }
        }

        private void HideVirtualCursor_Internal()
        {
            pointerVisible = false;

            if (virtualPointer != null)
            {
                virtualPointer.enabled = false;
            }
        }

        // ============================================================
        // GETTERS
        // ============================================================

        public static bool IsVirtualCursorVisible()
        {
            if (Instance == null)
            {
                return false;
            }

            return Instance.pointerVisible;
        }

        public static bool IsRealCursorVisible()
        {
            if (Instance == null)
            {
                return false;
            }

            return Instance.realCursorVisible;
        }

        // ============================================================
        // OPTIONAL REGISTER
        // ============================================================

        public void RegisterPointer(Image pointerImage)
        {
            virtualPointer = pointerImage;
        }
    }
}