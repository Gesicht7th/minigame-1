// Assets/_Game/Scripts/MemoryTest/TutorialReadyController.cs
// Mengelola input tutorial panel menggunakan dual wand controller.
// Taruh script ini sebagai component di GameObject TutorialPanel.
// Saat panel di-SetActive(false), OnDisable otomatis restore cursor.

using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.MemoryTest
{
    public class TutorialReadyController : MonoBehaviour
    {
        [Header("── Wand Readers ─────────────────────")]
        [Tooltip("WandSerialReader milik Player 1")]
        [SerializeField] private WandSerialReader p1Reader;
        [Tooltip("WandSerialReader milik Player 2")]
        [SerializeField] private WandSerialReader p2Reader;

        [Header("── UI References ────────────────────")]
        [Tooltip("MemoryTestUIManager di scene ini")]
        [SerializeField] private MemoryTestUIManager uiManager;
        [Tooltip("Tombol GO yang sudah ada (fallback mouse)")]
        [SerializeField] private Button goButton;
        [Tooltip("Image dengan Fill Method = Radial 360 untuk progress hold")]
        [SerializeField] private Image holdProgressBar;

        [Header("── Settings ─────────────────────────")]
        [Tooltip("Durasi hold dalam detik sebelum GO di-trigger")]
        [SerializeField] private float holdDuration = 1.5f;

        // State internal
        private float _holdTimer = 0f;
        private bool _triggered = false;
        private bool _dualWandMode = false;

        // ─────────────────────────────────────────────────────────
        void Start()
        {
            ResetProgressBar();
        }

        void Update()
        {
            // Setelah GO di-trigger, hentikan semua logic
            if (_triggered) return;

            bool p1Connected = p1Reader != null && p1Reader.IsConnected;
            bool p2Connected = p2Reader != null && p2Reader.IsConnected;
            bool dualConnected = p1Connected && p2Connected;

            // Deteksi perubahan status koneksi, update cursor & button
            if (dualConnected != _dualWandMode)
            {
                _dualWandMode = dualConnected;
                ApplyInputMode(useMouse: !dualConnected);
            }

            if (_dualWandMode)
            {
                HandleWandHold();
            }
            else
            {
                // Wand tidak terhubung: reset bar, biarkan mouse yang handle
                _holdTimer = 0f;
                ResetProgressBar();
            }
        }

        // ─────────────────────────────────────────────────────────
        /// <summary>
        /// Logika hold timer saat dual wand aktif.
        /// Kedua wand harus ditahan bersamaan selama holdDuration.
        /// </summary>
        private void HandleWandHold()
        {
            bool p1Held = p1Reader.IsActionHeld;
            bool p2Held = p2Reader.IsActionHeld;

            if (p1Held && p2Held)
            {
                _holdTimer += Time.deltaTime;
            }
            else
            {
                // Salah satu dilepas: reset timer
                _holdTimer = 0f;
            }

            // Update visual progress bar
            float progress = Mathf.Clamp01(_holdTimer / holdDuration);
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = progress;

            // Threshold tercapai: trigger GO
            if (_holdTimer >= holdDuration)
            {
                TriggerGo();
            }
        }

        // ─────────────────────────────────────────────────────────
        /// <summary>
        /// Atur cursor visibility dan interactability tombol GO
        /// sesuai mode input yang aktif.
        /// </summary>
        private void ApplyInputMode(bool useMouse)
        {
            // Cursor
            Cursor.visible = useMouse;
            Cursor.lockState = useMouse
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            // Tombol GO: aktif untuk mouse, nonaktif saat wand mode
            if (goButton != null)
                goButton.interactable = useMouse;

            // Reset bar dan timer saat mode berganti
            _holdTimer = 0f;
            ResetProgressBar();

            Debug.Log($"[TutorialReady] Mode: {(useMouse ? "Mouse" : "Dual Wand")}");
        }

        // ─────────────────────────────────────────────────────────
        /// <summary>
        /// Panggil ini untuk trigger GO secara programmatic.
        /// Bisa dipanggil dari wand hold ATAU dari luar jika diperlukan.
        /// </summary>
        public void TriggerGo()
        {
            if (_triggered) return;
            _triggered = true;

            // Isi penuh bar sebelum transisi
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 1f;

            Debug.Log("[TutorialReady] GO triggered!");
            uiManager?.TriggerTutorialDone();
        }

        // ─────────────────────────────────────────────────────────
        private void ResetProgressBar()
        {
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 0f;
        }

        /// <summary>
        /// Dipanggil otomatis saat Tutorial Panel di-SetActive(false).
        /// Pastikan cursor selalu dipulihkan meski scene berganti tiba-tiba.
        /// </summary>
        private void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _triggered = false;
            _holdTimer = 0f;
            ResetProgressBar();
        }
    }
}