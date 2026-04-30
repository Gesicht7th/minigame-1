// Assets/_Game/Scripts/ReflexShowdown/ReflexUIManager.cs

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.Reflex
{
    public class ReflexUIManager : MonoBehaviour
    {
        public static ReflexUIManager Instance { get; private set; }

        // ── Panels ─────────────────────────────────────
        [Header("── Panels ──────────────────────────────")]
        [SerializeField] private GameObject gameScreenPanel;
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private GameObject drawPanel;
        [SerializeField] private GameObject roundResultPanel;
        [SerializeField] private GameObject interRoundPanel;
        [SerializeField] private GameObject gameOverPanel;

        // ── HUD ────────────────────────────────────────
        [Header("── HUD ─────────────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundLabelText;
        [SerializeField] private TextMeshProUGUI p1WinsText;
        [SerializeField] private TextMeshProUGUI p2WinsText;

        // ── Ready ──────────────────────────────────────
        [Header("── Ready Panel ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI readyTitleText;
        [SerializeField] private Image p1ReadyIndicator;
        [SerializeField] private Image p2ReadyIndicator;
        [SerializeField] private Color readyColor = Color.green;
        [SerializeField] private Color notReadyColor = Color.red;

        // ── Countdown ──────────────────────────────────
        [Header("── Countdown ───────────────────────────")]
        [SerializeField] private TextMeshProUGUI countdownText;

        // ── Draw Phase ─────────────────────────────────
        [Header("── Draw Phase ──────────────────────────")]
        [SerializeField] private TextMeshProUGUI goText;
        [SerializeField] private TextMeshProUGUI p1TimerText;
        [SerializeField] private TextMeshProUGUI p2TimerText;

        // ── Round Result ───────────────────────────────
        [Header("── Round Result ────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundWinnerText;
        [SerializeField] private TextMeshProUGUI p1TimeText;
        [SerializeField] private TextMeshProUGUI p2TimeText;
        [SerializeField] private TextMeshProUGUI scoreboardText;
        [SerializeField] private TextMeshProUGUI falseStartText;

        // ── Inter Round ────────────────────────────────
        [Header("── Inter Round ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI interRoundScoreText;

        // ── Game Over ──────────────────────────────────
        [Header("── Game Over ───────────────────────────")]
        [SerializeField] private TextMeshProUGUI gameOverWinnerText;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Panel management ──────────────────────────
        public void HideAll()
        {
            gameScreenPanel?.SetActive(false);
            readyPanel?.SetActive(false);
            countdownPanel?.SetActive(false);
            drawPanel?.SetActive(false);
            roundResultPanel?.SetActive(false);
            interRoundPanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
        }

        public void ShowGameScreen()
        {
            gameScreenPanel?.SetActive(true);
            UpdateScoreboard(0, 0);
        }

        // ── Ready Phase ───────────────────────────────
        public void ShowReadyPrompt()
        {
            readyPanel?.SetActive(true);
            if (readyTitleText != null)
                readyTitleText.text = "HOLD WAND LOW!\nBoth players ready...";
        }

        public void UpdateReadyStatus(bool p1Ready, bool p2Ready)
        {
            if (p1ReadyIndicator != null)
                p1ReadyIndicator.color = p1Ready ? readyColor : notReadyColor;
            if (p2ReadyIndicator != null)
                p2ReadyIndicator.color = p2Ready ? readyColor : notReadyColor;
        }

        public void HideReadyPrompt() => readyPanel?.SetActive(false);

        // ── Countdown ────────────────────────────────
        public void ShowCountdown(string text)
        {
            countdownPanel?.SetActive(true);
            if (countdownText == null) return;
            countdownText.text = text;
            countdownText.transform.localScale = Vector3.one * 2f;
            StartCoroutine(ScaleTo(countdownText.transform, Vector3.one, 0.3f));
        }

        public void HideCountdown() => countdownPanel?.SetActive(false);

        // ── Draw / GO ─────────────────────────────────
        public void ShowGo()
        {
            drawPanel?.SetActive(true);
            if (goText != null)
            {
                goText.text = "DRAW!";
                goText.transform.localScale = Vector3.one * 3f;
                StartCoroutine(ScaleTo(goText.transform, Vector3.one, 0.2f));
            }
            if (p1TimerText != null) p1TimerText.text = "---";
            if (p2TimerText != null) p2TimerText.text = "---";
        }

        public void UpdateDrawTimers(float t1, float t2, bool p1Fired, bool p2Fired)
        {
            if (p1TimerText != null)
                p1TimerText.text = p1Fired ? $"{t1:F3}s" : $"{t1:F2}s...";
            if (p2TimerText != null)
                p2TimerText.text = p2Fired ? $"{t2:F3}s" : $"{t2:F2}s...";
        }

        public void HideGo() => drawPanel?.SetActive(false);

        // ── Round Result ──────────────────────────────
        public void ShowRoundResult(int winner, float t1, float t2, int p1wins, int p2wins)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(false);

            if (roundWinnerText != null)
            {
                roundWinnerText.text = winner == 0 ? "DRAW!"
                    : $"PLAYER {winner} WINS!";
                roundWinnerText.color = winner == 1
                    ? new Color(0.3f, 0.6f, 1f)
                    : winner == 2
                    ? new Color(1f, 0.4f, 0.2f)
                    : Color.white;
            }

            if (p1TimeText != null)
                p1TimeText.text = t1 >= 0 ? $"P1: {t1:F3}s" : "P1: NO FIRE";
            if (p2TimeText != null)
                p2TimeText.text = t2 >= 0 ? $"P2: {t2:F3}s" : "P2: NO FIRE";

            UpdateScoreboard(p1wins, p2wins);
        }

        public void ShowFalseStartResult(bool p1False, bool p2False, int p1wins, int p2wins)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(true);

            if (falseStartText != null)
            {
                string who = (p1False && p2False) ? "BOTH PLAYERS"
                    : p1False ? "PLAYER 1" : "PLAYER 2";
                falseStartText.text = $"FALSE START!\n{who} fired early!";
                falseStartText.color = new Color(1f, 0.8f, 0f);
            }

            if (roundWinnerText != null)
            {
                int winner = (p1False && !p2False) ? 2 : (!p1False && p2False) ? 1 : 0;
                roundWinnerText.text = winner == 0 ? "DRAW!" : $"PLAYER {winner} WINS!";
            }

            UpdateScoreboard(p1wins, p2wins);
        }

        public void HideRoundResult() => roundResultPanel?.SetActive(false);

        // ── Inter Round ───────────────────────────────
        public void ShowInterRound(int p1wins, int p2wins)
        {
            interRoundPanel?.SetActive(true);
            if (interRoundScoreText != null)
                interRoundScoreText.text = $"P1: {p1wins}  vs  P2: {p2wins}";
        }

        public void HideInterRound() => interRoundPanel?.SetActive(false);

        // ── Game Over ─────────────────────────────────
        public void ShowGameOver(int winner, int p1wins, int p2wins)
        {
            gameOverPanel?.SetActive(true);
            if (gameOverWinnerText != null)
                gameOverWinnerText.text = winner == 0
                    ? "IT'S A TIE!" : $"PLAYER {winner}\nWINS THE MATCH!";
            if (gameOverScoreText != null)
                gameOverScoreText.text = $"P1: {p1wins} wins  |  P2: {p2wins} wins";
        }

        // ── HUD Helpers ───────────────────────────────
        public void UpdateRoundLabel(int cur, int total)
        {
            if (roundLabelText != null)
                roundLabelText.text = $"ROUND {cur} / {total}";
        }

        private void UpdateScoreboard(int p1wins, int p2wins)
        {
            if (p1WinsText != null) p1WinsText.text = p1wins.ToString();
            if (p2WinsText != null) p2WinsText.text = p2wins.ToString();
            if (scoreboardText != null)
                scoreboardText.text = $"P1: {p1wins}  —  P2: {p2wins}";
        }

        // ── Animation Helper ──────────────────────────
        private IEnumerator ScaleTo(Transform t, Vector3 target, float dur)
        {
            Vector3 start = t.localScale;
            float el = 0f;
            while (el < dur)
            {
                el += Time.deltaTime;
                t.localScale = Vector3.Lerp(start, target, el / dur);
                yield return null;
            }
            t.localScale = target;
        }
    }
}