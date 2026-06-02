// Assets/_Game/Scripts/UI/ReadyScreenUI.cs

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ReadyScreenUI : MonoBehaviour
    {
        [Header("── Layar Pemain 1 (Kiri) ───────────────")]
        [SerializeField] private Image unreadyImage1;
        [SerializeField] private Image readyImage1;
        [SerializeField] private TextMeshProUGUI p1StatusText;

        [Header("── Layar Pemain 2 (Kanan) ──────────────")]
        [SerializeField] private Image unreadyImage2;
        [SerializeField] private Image readyImage2;
        [SerializeField] private TextMeshProUGUI p2StatusText;

        [Header("── Teks Utama ──────────────────────────")]
        [SerializeField] private TextMeshProUGUI bothReadyText;

        [Header("── Tombol ──────────────────────────────")]
        [SerializeField] private Button backButton;

        [Header("── Hitung Mundur ───────────────────────")]
        [SerializeField] private float countdownBeforeStart = 3f;

        // ── TAMBAHAN: Wand Input ──────────────────────────────────
        [Header("── Wand Input (opsional) ─────────────────")]
        [Tooltip("Wand P1 — action button = toggle ready P1")]
        [SerializeField] private WandSerialReader p1WandReader;
        [Tooltip("Wand P2 — action button = toggle ready P2")]
        [SerializeField] private WandSerialReader p2WandReader;
        // ─────────────────────────────────────────────────────────

        // Status
        private bool p1Ready = false;
        private bool p2Ready = false;
        private bool startingGame = false;

        void Start()
        {
            if (p1WandReader == null)
            {
                p1WandReader = WandSerialReader.GetByPort("COM8");
                if (p1WandReader != null) Debug.Log("[ReaderResolve] SUCCESS COM8");
                else Debug.Log("[ReaderResolve] FAILED COM8");
            }
            if (p2WandReader == null)
            {
                p2WandReader = WandSerialReader.GetByPort("COM9");
                if (p2WandReader != null) Debug.Log("[ReaderResolve] SUCCESS COM9");
                else Debug.Log("[ReaderResolve] FAILED COM9");
            }

            UpdateIndicators();
            if (bothReadyText != null) bothReadyText.gameObject.SetActive(false);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                    SceneFlowManager.Instance.GoTo(SceneNames.MainMenu));
            }
        }

        void Update()
        {
            if (startingGame) return;

            // ── Pemain 1 siap: Spasi ATAU Wand P1 Action ─────────
            bool p1Input = Input.GetKeyDown(KeyCode.Space)
                        || (p1WandReader != null && p1WandReader.ConsumeAction());

            if (p1Input)
            {
                p1Ready = !p1Ready;
                UpdateIndicators();
                if (p1Ready && readyImage1 != null)
                    StartCoroutine(PopAnimation(readyImage1.transform));
            }

            // ── Pemain 2 siap: Enter ATAU Wand P2 Action ─────────
            bool p2Input = (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        || (p2WandReader != null && p2WandReader.ConsumeAction());

            if (p2Input)
            {
                p2Ready = !p2Ready;
                UpdateIndicators();
                if (p2Ready && readyImage2 != null)
                    StartCoroutine(PopAnimation(readyImage2.transform));
            }

            // ── Escape = batal semua (keyboard only, untuk debugging) ──
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                p1Ready = false;
                p2Ready = false;
                UpdateIndicators();
            }

            // ── Kedua pemain siap → mulai hitung mundur ───────────
            if (p1Ready && p2Ready && !startingGame)
            {
                startingGame = true;
                StartCoroutine(StartCountdown());
            }
        }

        // ── Semua method di bawah tidak diubah sama sekali ────────

        private void UpdateIndicators()
        {
            if (unreadyImage1 != null) unreadyImage1.gameObject.SetActive(!p1Ready);
            if (readyImage1 != null) readyImage1.gameObject.SetActive(p1Ready);

            if (unreadyImage2 != null) unreadyImage2.gameObject.SetActive(!p2Ready);
            if (readyImage2 != null) readyImage2.gameObject.SetActive(p2Ready);

            if (p1StatusText != null)
            {
                p1StatusText.text = p1Ready ? "READY" : "NOT READY";
                p1StatusText.color = p1Ready ? Color.green : Color.red;
            }

            if (p2StatusText != null)
            {
                p2StatusText.text = p2Ready ? "READY" : "NOT READY";
                p2StatusText.color = p2Ready ? Color.green : Color.red;
            }
        }

        private IEnumerator StartCountdown()
        {
            if (bothReadyText != null) bothReadyText.gameObject.SetActive(true);

            for (int i = (int)countdownBeforeStart; i >= 1; i--)
            {
                if (bothReadyText != null) bothReadyText.text = $"Starting in {i}...";
                yield return new WaitForSeconds(1f);

                if (!p1Ready || !p2Ready)
                {
                    startingGame = false;
                    if (bothReadyText != null) bothReadyText.gameObject.SetActive(false);
                    UpdateIndicators();
                    yield break;
                }
            }

            SceneFlowManager.Instance.GoTo(SceneNames.MemoryTest);
        }

        private IEnumerator PopAnimation(Transform target)
        {
            target.localScale = Vector3.zero;

            float timer = 0f;
            float duration = 0.35f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

                target.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easedT);

                yield return null;
            }

            target.localScale = Vector3.one;
        }
    }
}