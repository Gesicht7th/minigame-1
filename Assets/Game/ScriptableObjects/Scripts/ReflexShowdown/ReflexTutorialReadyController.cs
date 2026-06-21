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
        [SerializeField] private Button goButton;
        [SerializeField] private Image holdProgressBar;

        [Header("── Settings ─────────────────────────")]
        [SerializeField] private float holdDuration = 1.5f;

        private float _holdTimer = 0f;
        private bool _triggered = false;
        private bool _dualWandMode = false;

        void Start()
        {
            if (!WandSerialReader.IsAlive(p1Reader)) p1Reader = PlayerAssignment.PlayerA;
            if (!WandSerialReader.IsAlive(p2Reader)) p2Reader = PlayerAssignment.PlayerB;
            ResetProgressBar();
        }

        void Update()
        {
            if (_triggered) return;

            bool p1Connected = p1Reader != null && p1Reader.IsConnected;
            bool p2Connected = p2Reader != null && p2Reader.IsConnected;
            bool dualConnected = p1Connected && p2Connected;

            if (dualConnected != _dualWandMode)
            {
                _dualWandMode = dualConnected;
                ApplyInputMode(useMouse: !dualConnected);
            }

            HandleHoldLogic();
        }

        private void HandleHoldLogic()
        {
            bool p1Held = DebugInputManager.GetActionHeld(PlayerSide.PlayerA);
            bool p2Held = DebugInputManager.GetActionHeld(PlayerSide.PlayerB);
            bool keyboardHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return);

            // PERBAIKAN FLEKSIBILITAS INPUT: 
            // Jika dual wand tersambung, wajib kedua alat menahan.
            // Jika tidak, mengizinkan pengetesan solo dengan keyboard/satu controller.
            bool isHolding = _dualWandMode ? (p1Held && p2Held) : (p1Held || p2Held || keyboardHeld);

            if (isHolding)
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

            // PERBAIKAN BYPASS BUTTON INACTIVE:
            // Panggil fungsi secara langsung ke UIManager terlepas dari status visibilitas panel saat ini.
            if (uiManager != null)
            {
                uiManager.TriggerTutorialDone();
            }

            // Tetap lakukan invoke ke button (untuk kompatibilitas) HANYA JIKA aktif.
            if (goButton != null && goButton.gameObject.activeInHierarchy)
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