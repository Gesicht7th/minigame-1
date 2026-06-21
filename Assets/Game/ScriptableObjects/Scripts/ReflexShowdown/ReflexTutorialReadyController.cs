// Assets/Game/ScriptableObjects/Scripts/ReflexShowdown/ReflexTutorialReadyController.cs

using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.Reflex
{
    public class ReflexTutorialReadyController : MonoBehaviour
    {
        [Header("── Wand Readers ─────────────────────")]
        [SerializeField] private WandSerialReader p1Reader;
        [SerializeField] private WandSerialReader p2Reader;

        [Header("── UI References ────────────────────")]
        [SerializeField] private ReflexUIManager uiManager;
        [SerializeField] private Button goButton; // Ini referensi ke tutorialGoButton
        [SerializeField] private Image holdProgressBar;

        [Header("── Settings ─────────────────────────")]
        [SerializeField] private float holdDuration = 1.5f;

        private float _holdTimer = 0f;
        private bool _triggered = false;
        private bool _dualWandMode = false;

        void Start()
        {
            // Auto-resolve reader if missing
            if (!WandSerialReader.IsAlive(p1Reader)) p1Reader = PlayerAssignment.PlayerA;
            if (!WandSerialReader.IsAlive(p2Reader)) p2Reader = PlayerAssignment.PlayerB;
            ResetProgressBar();
        }

        void Update()
        {
            if (_triggered) return;

            // Fitur "Hold" harus bisa dari Slide 1 maupun Slide 2
            // Jadi script ini harus tetap menyala.

            bool p1Connected = p1Reader != null && p1Reader.IsConnected;
            bool p2Connected = p2Reader != null && p2Reader.IsConnected;
            bool dualConnected = p1Connected && p2Connected;

            if (dualConnected != _dualWandMode)
            {
                _dualWandMode = dualConnected;
                ApplyInputMode(useMouse: !dualConnected);
            }

            // Selalu deteksi input hold (Keyboard Spasi / Wand)
            HandleHoldLogic();
        }

        private void HandleHoldLogic()
        {
            // DebugInputManager sudah meng-handle ESP32 Wand dan Keyboard (Spasi)
            bool p1Held = DebugInputManager.GetActionHeld(PlayerSide.PlayerA);
            bool p2Held = DebugInputManager.GetActionHeld(PlayerSide.PlayerB);

            if (p1Held && p2Held)
            {
                _holdTimer += Time.deltaTime;
            }
            else
            {
                _holdTimer = 0f;
            }

            float progress = Mathf.Clamp01(_holdTimer / holdDuration);
            if (holdProgressBar != null) holdProgressBar.fillAmount = progress;

            if (_holdTimer >= holdDuration)
            {
                TriggerGo();
            }
        }

        private void ApplyInputMode(bool useMouse)
        {
            if (goButton != null) goButton.interactable = useMouse;
            _holdTimer = 0f;
            ResetProgressBar();
        }

        public void TriggerGo()
        {
            if (_triggered) return;
            _triggered = true;

            if (holdProgressBar != null) holdProgressBar.fillAmount = 1f;

            if (goButton != null)
            {
                goButton.onClick.Invoke();
            }
        }

        private void ResetProgressBar()
        {
            if (holdProgressBar != null) holdProgressBar.fillAmount = 0f;
        }

        private void OnDisable()
        {
            _triggered = false;
            _holdTimer = 0f;
            ResetProgressBar();
        }
    }
}