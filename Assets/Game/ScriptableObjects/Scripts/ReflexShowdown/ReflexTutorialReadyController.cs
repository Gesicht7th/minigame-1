// Assets/Game/ScriptableObjects/Scripts/ReflexShowdown/ReflexTutorialReadyController.cs
// Mengelola input tutorial panel menggunakan dual wand controller untuk ReflexShowdown.
// Taruh script ini sebagai component di GameObject TutorialPanel atau UI Controller.

using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.Reflex
{
    public class ReflexTutorialReadyController : MonoBehaviour
    {
        [Header("── Wand Readers ─────────────────────")]
        [Tooltip("WandSerialReader milik Player 1")]
        [SerializeField] private WandSerialReader p1Reader;
        [Tooltip("WandSerialReader milik Player 2")]
        [SerializeField] private WandSerialReader p2Reader;

        [Header("── UI References ────────────────────")]
        [Tooltip("ReflexUIManager di scene ini")]
        [SerializeField] private ReflexUIManager uiManager;
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
            if (p1Reader == null)
            {
                p1Reader = WandSerialReader.GetByPort("COM8");
                Debug.Log("[ReaderResolve] Player1 -> COM8");
            }
            if (p2Reader == null)
            {
                p2Reader = WandSerialReader.GetByPort("COM9");
                Debug.Log("[ReaderResolve] Player2 -> COM9");
            }

            ResetProgressBar();
        }

        void Update()
        {
            // Setelah GO di-trigger, hentikan semua logic
            if (_triggered) return;

            bool p1Connected = p1Reader != null && p1Reader.IsConnected;
            bool p2Connected = p2Reader != null && p2Reader.IsConnected;
            bool dualConnected = p1Connected && p2Connected;

            // Deteksi perubahan status koneksi, update button
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
            bool p1Held = p1Reader.IsHolding;  
            bool p2Held = p2Reader.IsHolding;  

            if (p1Held && p2Held)
            {
                _holdTimer += Time.deltaTime;
            }
            else
            {
                _holdTimer = 0f;
            }

            float progress = Mathf.Clamp01(_holdTimer / holdDuration);
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = progress;

            if (_holdTimer >= holdDuration)
            {
                TriggerGo();
            }
        }

        // ─────────────────────────────────────────────────────────
        /// <summary>
        /// Atur interactability tombol GO sesuai mode input yang aktif.
        /// </summary>
        private void ApplyInputMode(bool useMouse)
        {
            // Tombol GO: aktif untuk mouse, nonaktif saat wand mode
            if (goButton != null)
                goButton.interactable = useMouse;

            // Reset bar dan timer saat mode berganti
            _holdTimer = 0f;
            ResetProgressBar();
        }

        // ─────────────────────────────────────────────────────────
        /// <summary>
        /// Panggil ini untuk trigger GO secara programmatic.
        /// </summary>
        public void TriggerGo()
        {
            if (_triggered) return;
            _triggered = true;

            // Isi penuh bar sebelum transisi
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 1f;

            // Memanfaatkan delegate di tutorialGoButton.onClick via OnClick()
            // Karena ReflexUIManager.IsTutorialDone hanya bisa di-set dari UI
            if (goButton != null)
            {
                goButton.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning("[ReflexTutorial] GO Button referensi kosong, gagal menutup tutorial.");
            }
        }

        // ─────────────────────────────────────────────────────────
        private void ResetProgressBar()
        {
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 0f;
        }

        private void OnDisable()
        {
            _triggered = false;
            _holdTimer = 0f;
            ResetProgressBar();
        }
    }
}
