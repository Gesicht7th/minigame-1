// Assets/_Game/Scripts/UI/ReadyScreenUI.cs

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ReadyScreenUI : MonoBehaviour
    {
        [Header("── Ready Indicators ────────────────────")]
        [SerializeField] private Image p1StatusCircle;
        [SerializeField] private Image p2StatusCircle;
        [SerializeField] private TextMeshProUGUI bothReadyText;

        [Header("── Colors ───────────────────────────────")]
        [SerializeField] private Color readyColor = new Color(0.1f, 0.9f, 0.3f);
        [SerializeField] private Color notReadyColor = new Color(0.9f, 0.2f, 0.2f);

        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button backButton;

        [Header("── Countdown ───────────────────────────")]
        [SerializeField] private float countdownBeforeStart = 3f;

        // State
        private bool p1Ready = false;
        private bool p2Ready = false;
        private bool startingGame = false;

        void Start()
        {
            UpdateIndicators();
            bothReadyText?.gameObject.SetActive(false);

            backButton?.onClick.AddListener(() =>
                SceneFlowManager.Instance.GoTo(SceneNames.MainMenu));
        }

        void Update()
        {
            if (startingGame) return;

            // Player 1 ready: Space
            if (Input.GetKeyDown(KeyCode.Space))
                p1Ready = !p1Ready;

            // Player 2 ready: Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                p2Ready = !p2Ready;

            // Escape = cancel semua
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                p1Ready = false;
                p2Ready = false;
            }

            UpdateIndicators();

            // Keduanya siap → mulai countdown
            if (p1Ready && p2Ready && !startingGame)
            {
                startingGame = true;
                StartCoroutine(StartCountdown());
            }
        }

        private void UpdateIndicators()
        {
            if (p1StatusCircle) p1StatusCircle.color = p1Ready ? readyColor : notReadyColor;
            if (p2StatusCircle) p2StatusCircle.color = p2Ready ? readyColor : notReadyColor;
        }

        private IEnumerator StartCountdown()
        {
            bothReadyText?.gameObject.SetActive(true);

            for (int i = (int)countdownBeforeStart; i >= 1; i--)
            {
                if (bothReadyText) bothReadyText.text = $"Starting in {i}...";
                yield return new WaitForSeconds(1f);

                // Jika salah satu cancel
                if (!p1Ready || !p2Ready)
                {
                    startingGame = false;
                    bothReadyText?.gameObject.SetActive(false);
                    yield break;
                }
            }

            // Mulai Game 1
            SceneFlowManager.Instance.GoTo(SceneNames.MemoryTest);
        }
    }
}