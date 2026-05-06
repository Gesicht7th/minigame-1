// Assets/_Game/Scripts/UI/EndScreenUI.cs
// Final screen — tampilkan skor gabungan dari 3 game, 2 player

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WizardPunk
{
    public class EndScreenUI : MonoBehaviour
    {
        [Header("── Player 1 Stats ──────────────────────")]
        [SerializeField] private TextMeshProUGUI p1TotalScoreText;
        [SerializeField] private TextMeshProUGUI p1Game1ScoreText;
        [SerializeField] private TextMeshProUGUI p1Game2ScoreText;
        [SerializeField] private TextMeshProUGUI p1Game3ScoreText;

        [Header("── Player 2 Stats ──────────────────────")]
        [SerializeField] private TextMeshProUGUI p2TotalScoreText;
        [SerializeField] private TextMeshProUGUI p2Game1ScoreText;
        [SerializeField] private TextMeshProUGUI p2Game2ScoreText;
        [SerializeField] private TextMeshProUGUI p2Game3ScoreText;

        [Header("── Overall Winner ──────────────────────")]
        [SerializeField] private TextMeshProUGUI overallWinnerText;
        [SerializeField] private GameObject p1WinBadge;
        [SerializeField] private GameObject p2WinBadge;
        [SerializeField] private GameObject drawBadge;

        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;

        void Start()
        {
            PopulateAll();
            playAgainButton?.onClick.AddListener(() =>
            {
                GameDataBridge.Instance?.ResetAll();
                SceneFlowManager.Instance.GoTo(SceneNames.MainMenu);
            });

            menuButton?.onClick.AddListener(() =>
                SceneFlowManager.Instance.GoTo(SceneNames.MainMenu));
        }

        void PopulateAll()
        {
            var b = GameDataBridge.Instance;
            if (b == null) return;

            // ── Memory Test (Game 1) ──────────────────
            // Game 1 adalah single player untuk Memory Test
            int mg1Score = b.MemoryTestScore;
            if (p1Game1ScoreText) p1Game1ScoreText.text = $"Memory Test: {mg1Score}";

            // ── Hyper Smash (Game 2) ──────────────────
            int mg2Score = b.HyperSmashScore;
            if (p1Game2ScoreText) p1Game2ScoreText.text = $"Hyper Smash: {mg2Score}";

            // ── Reflex Showdown (Game 3) ──────────────
            int p1Reflex = b.ReflexP1Points;
            int p2Reflex = b.ReflexP2Points;
            if (p1Game3ScoreText) p1Game3ScoreText.text = $"Reflex: {p1Reflex}";
            if (p2Game1ScoreText) p2Game1ScoreText.text = $"Memory Test: {mg1Score}";
            if (p2Game2ScoreText) p2Game2ScoreText.text = $"Hyper Smash: {mg2Score}";
            if (p2Game3ScoreText) p2Game3ScoreText.text = $"Reflex: {p2Reflex}";

            // ── Total ─────────────────────────────────
            // Game 1 & 2 = shared (single player), Game 3 = per-player
            int p1Total = mg1Score + mg2Score + p1Reflex;
            int p2Total = mg1Score + mg2Score + p2Reflex;

            if (p1TotalScoreText) p1TotalScoreText.text = p1Total.ToString();
            if (p2TotalScoreText) p2TotalScoreText.text = p2Total.ToString();

            // ── Overall Winner ────────────────────────
            int overallWinner = p1Total > p2Total ? 1 : p2Total > p1Total ? 2 : 0;

            if (overallWinnerText != null)
                overallWinnerText.text = overallWinner == 0
                    ? "IT'S A TIE!"
                    : $"PLAYER {overallWinner}\nIS THE CHAMPION!";

            p1WinBadge?.SetActive(overallWinner == 1);
            p2WinBadge?.SetActive(overallWinner == 2);
            drawBadge?.SetActive(overallWinner == 0);
        }
    }
}