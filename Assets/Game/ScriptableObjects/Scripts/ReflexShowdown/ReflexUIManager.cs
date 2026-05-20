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

        [Header("── Panels ──────────────────────────────")]
        [SerializeField] private GameObject gameScreenPanel;
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private GameObject drawPanel;
        [SerializeField] private GameObject roundResultPanel;
        [SerializeField] private GameObject interRoundPanel;

        [Header("── Hearts UI ───────────────────────────")]
        [SerializeField] private GameObject[] p1HeartIcons;
        [SerializeField] private GameObject[] p2HeartIcons;

        // --- TAMBAHAN UNTUK HIGHLIGHT BATU-GUNTING-KERTAS ---
        [Header("── RPS Action UI ───────────────────────")]
        [Tooltip("0: Block(Rock), 1: Penetration(Paper), 2: Counter(Scissors)")]
        [SerializeField] private Image[] p1ActionIcons;
        [SerializeField] private Image[] p2ActionIcons;
        [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f); // Redup
        [SerializeField] private Color selectedColor = Color.white;                         // Terang
        [SerializeField] private float unselectedScale = 0.8f;                              // Mengecil
        [SerializeField] private float selectedScale = 1.1f;                                // Membesar
        // ----------------------------------------------------

        [Header("── Result Pop-Up ──")]
        [SerializeField] private GameObject popupBackground;
        [SerializeField] private GameObject p1WinPanel;
        [SerializeField] private GameObject p2WinPanel;
        [SerializeField] private GameObject drawWinPanel;

        [SerializeField] private Button p1NextButton;
        [SerializeField] private Button p2NextButton;
        [SerializeField] private Button drawNextButton;

        [SerializeField] private string nextSceneName = "MainMenu";

        [Header("── HUD ─────────────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundLabelText;

        [Header("── Ready Panel ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI readyTitleText;
        [SerializeField] private Image p1ReadyIndicator;
        [SerializeField] private Image p2ReadyIndicator;
        [SerializeField] private Color readyColor = Color.green;
        [SerializeField] private Color notReadyColor = Color.red;

        [Header("── Countdown ───────────────────────────")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("── Draw Phase ──────────────────────────")]
        [SerializeField] private TextMeshProUGUI goText;
        [SerializeField] private TextMeshProUGUI p1TimerText;
        [SerializeField] private TextMeshProUGUI p2TimerText;

        [Header("── Round Result ────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundWinnerText;
        [SerializeField] private TextMeshProUGUI p1TimeText;
        [SerializeField] private TextMeshProUGUI p2TimeText;
        [SerializeField] private TextMeshProUGUI falseStartText;

        [Header("── Inter Round ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI interRoundTitleText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (ReflexScoreManager.Instance != null)
                ReflexScoreManager.Instance.OnHeartsUpdated += UpdateHeartsUI;

            if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
            if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
            if (drawNextButton != null) drawNextButton.onClick.AddListener(GoToNextGame);
        }

        void OnDestroy()
        {
            if (ReflexScoreManager.Instance != null)
                ReflexScoreManager.Instance.OnHeartsUpdated -= UpdateHeartsUI;
        }

        public void HideAll()
        {
            gameScreenPanel?.SetActive(false);
            readyPanel?.SetActive(false);
            countdownPanel?.SetActive(false);
            drawPanel?.SetActive(false);
            roundResultPanel?.SetActive(false);
            interRoundPanel?.SetActive(false);

            popupBackground?.SetActive(false);
        }

        public void ShowGameScreen()
        {
            gameScreenPanel?.SetActive(true);
        }

        // --- FUNGSI BARU: Mengatur Visual RPS UI ---
        public void ResetActionUI(int playerId)
        {
            Image[] icons = (playerId == 1) ? p1ActionIcons : p2ActionIcons;
            if (icons == null) return;
            foreach (var icon in icons)
            {
                if (icon != null)
                {
                    icon.color = unselectedColor;
                    icon.transform.localScale = Vector3.one * unselectedScale;
                }
            }
        }

        public void UpdateActionUI(int playerId, RpsType selectedAction)
        {
            Image[] icons = (playerId == 1) ? p1ActionIcons : p2ActionIcons;
            if (icons == null || icons.Length < 3) return;

            int selectedIndex = -1;
            if (selectedAction == RpsType.Rock) selectedIndex = 0;
            else if (selectedAction == RpsType.Paper) selectedIndex = 1;
            else if (selectedAction == RpsType.Scissors) selectedIndex = 2;

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                {
                    bool isSelected = (i == selectedIndex);
                    icons[i].color = isSelected ? selectedColor : unselectedColor;
                    icons[i].transform.localScale = Vector3.one * (isSelected ? selectedScale : unselectedScale);
                }
            }
        }
        // ------------------------------------------

        // --- SISTEM HEARTS ---
        private void UpdateHeartsUI(int p1Hearts, int p2Hearts)
        {
            for (int i = 0; i < p1HeartIcons.Length; i++)
            {
                if (p1HeartIcons[i] != null)
                    p1HeartIcons[i].SetActive(i < p1Hearts);
            }

            for (int i = 0; i < p2HeartIcons.Length; i++)
            {
                if (p2HeartIcons[i] != null)
                    p2HeartIcons[i].SetActive(i < p2Hearts);
            }
        }

        // --- RESULT POP UP ---
        public void ShowResultPopup(int winner)
        {
            HideAll();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (popupBackground != null) popupBackground.SetActive(true);

            if (winner == 1 && p1WinPanel != null) p1WinPanel.SetActive(true);
            else if (winner == 2 && p2WinPanel != null) p2WinPanel.SetActive(true);
            else if (winner == 0 && drawWinPanel != null) drawWinPanel.SetActive(true);
        }

        private void GoToNextGame()
        {
            SceneFlowManager.Instance.GoTo(nextSceneName);
        }

        // ── Phase UI Methods ─────────────────────────
        public void ShowReadyPrompt()
        {
            readyPanel?.SetActive(true);
            if (readyTitleText != null) readyTitleText.text = "HOLD WAND LOW!\nBoth players ready...";
        }

        public void UpdateReadyStatus(bool p1Ready, bool p2Ready)
        {
            if (p1ReadyIndicator != null) p1ReadyIndicator.color = p1Ready ? readyColor : notReadyColor;
            if (p2ReadyIndicator != null) p2ReadyIndicator.color = p2Ready ? readyColor : notReadyColor;
        }

        public void HideReadyPrompt() => readyPanel?.SetActive(false);

        public void ShowCountdown(string text)
        {
            countdownPanel?.SetActive(true);
            if (countdownText == null) return;
            countdownText.text = text;
            countdownText.transform.localScale = Vector3.one * 2f;
            StartCoroutine(ScaleTo(countdownText.transform, Vector3.one, 0.3f));
        }

        public void HideCountdown() => countdownPanel?.SetActive(false);

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
            if (p1TimerText != null) p1TimerText.text = p1Fired ? $"{t1:F3}s" : $"{t1:F2}s...";
            if (p2TimerText != null) p2TimerText.text = p2Fired ? $"{t2:F3}s" : $"{t2:F2}s...";
        }

        public void HideGo() => drawPanel?.SetActive(false);

        public void ShowRoundResult(int winner, float t1, float t2)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(false);

            if (roundWinnerText != null)
            {
                roundWinnerText.text = winner == 0 ? "DRAW!" : $"PLAYER {winner} WINS!";
                roundWinnerText.color = winner == 1 ? new Color(0.3f, 0.6f, 1f) : winner == 2 ? new Color(1f, 0.4f, 0.2f) : Color.white;
            }

            if (p1TimeText != null) p1TimeText.text = t1 >= 0 ? $"P1: {t1:F3}s" : "P1: NO FIRE";
            if (p2TimeText != null) p2TimeText.text = t2 >= 0 ? $"P2: {t2:F3}s" : "P2: NO FIRE";
        }

        public void ShowFalseStartResult(bool p1False, bool p2False)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(true);

            if (falseStartText != null)
            {
                string who = (p1False && p2False) ? "BOTH PLAYERS" : p1False ? "PLAYER 1" : "PLAYER 2";
                falseStartText.text = $"FALSE START!\n{who} fired early!";
                falseStartText.color = new Color(1f, 0.8f, 0f);
            }

            if (roundWinnerText != null)
            {
                int winner = (p1False && !p2False) ? 2 : (!p1False && p2False) ? 1 : 0;
                roundWinnerText.text = winner == 0 ? "DRAW!" : $"PLAYER {winner} WINS!";
            }
        }

        public void HideRoundResult() => roundResultPanel?.SetActive(false);

        public void ShowInterRound(int round)
        {
            interRoundPanel?.SetActive(true);
            if (interRoundTitleText != null) interRoundTitleText.text = $"ROUND {round} COMPLETE";
        }

        public void HideInterRound() => interRoundPanel?.SetActive(false);

        public void UpdateRoundLabel(int cur)
        {
            if (roundLabelText != null) roundLabelText.text = $"ROUND {cur}";
        }

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