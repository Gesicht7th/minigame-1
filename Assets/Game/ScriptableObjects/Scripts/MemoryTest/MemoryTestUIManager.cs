// Assets/_Game/Scripts/MemoryTest/MemoryTestUIManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestUIManager : MonoBehaviour
    {
        public static MemoryTestUIManager Instance { get; private set; }

        [Header("HUD Score")]
        [SerializeField] private TextMeshProUGUI scoreP1Text;
        [SerializeField] private TextMeshProUGUI scoreP2Text;

        [Header("HUD Center & Difficulty")]
        [SerializeField] private TextMeshProUGUI centerPhaseText;
        [SerializeField] private TextMeshProUGUI difficultyText;

        [Header("HUD Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillBar;

        [Header("── Tutorial ──")]
        [SerializeField] private GameObject tutorialPanel;   // Panel Tutorial
        [SerializeField] private Button tutorialGoButton;    // Tombol GO

        [Header("── Times Up Pop-Up ──")]
        [SerializeField] private GameObject timesUpPanel;    // Panel Times Up

        [Header("── Result Pop-Up ──")]
        [SerializeField] private GameObject popupBackground;
        [SerializeField] private GameObject p1WinPanel;
        [SerializeField] private GameObject p2WinPanel;
        [SerializeField] private GameObject drawWinPanel;

        [SerializeField] private TextMeshProUGUI p1PtsText;
        [SerializeField] private TextMeshProUGUI p2PtsText;
        [SerializeField] private TextMeshProUGUI drawPtsText;

        [SerializeField] private Button p1NextButton;
        [SerializeField] private Button p2NextButton;
        [SerializeField] private Button drawNextButton;

        [SerializeField] private string nextSceneName = "test hypersmash";

        public bool IsTutorialDone { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            MemoryTestScoreManager.Instance.OnScoreUpdated += UpdateScoreUI;

            if (popupBackground != null) popupBackground.SetActive(false);
            if (timesUpPanel != null) timesUpPanel.SetActive(false); // Pastikan panel tertutup di awal

            if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
            if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
            if (drawNextButton != null) drawNextButton.onClick.AddListener(GoToNextGame);

            // --- FUNGSI BARU: Tombol GO Tutorial ---
            if (tutorialGoButton != null)
            {
                tutorialGoButton.onClick.AddListener(() =>
                {
                    IsTutorialDone = true;
                });
            }
        }

        void OnDestroy()
        {
            if (MemoryTestScoreManager.Instance != null)
                MemoryTestScoreManager.Instance.OnScoreUpdated -= UpdateScoreUI;
        }

        // --- FUNGSI BARU: Tampilkan & Sembunyikan Tutorial ---
        public void ShowTutorial()
        {
            IsTutorialDone = false;
            if (tutorialPanel != null) tutorialPanel.SetActive(true);
        }

        public void HideTutorial()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }
        // -----------------------------------------------------

        public void TriggerTutorialDone()
        {
            IsTutorialDone = true;
        }

        // --- FUNGSI BARU: Tampilkan & Sembunyikan Times Up ---
        public void ShowTimesUp()
        {
            if (timesUpPanel != null) timesUpPanel.SetActive(true);
        }

        public void HideTimesUp()
        {
            if (timesUpPanel != null) timesUpPanel.SetActive(false);
        }
        // -----------------------------------------------------

        private void UpdateScoreUI(int playerId, int score)
        {
            if (playerId == 1 && scoreP1Text != null) scoreP1Text.text = score.ToString();
            if (playerId == 2 && scoreP2Text != null) scoreP2Text.text = score.ToString();
        }

        public void UpdateDifficultyText(string diff)
        {
            if (difficultyText != null) difficultyText.text = diff;
        }

        public void UpdateTimerUI(float currentTime, float maxTime)
        {
            if (timerText != null) timerText.text = $"{Mathf.CeilToInt(currentTime)}s";
            if (timerFillBar != null) timerFillBar.fillAmount = currentTime / maxTime;
        }

        public void ShowCenterText(string msg)
        {
            if (centerPhaseText != null)
            {
                centerPhaseText.text = msg;
                centerPhaseText.gameObject.SetActive(true);
            }
        }

        public void HideCenterText()
        {
            if (centerPhaseText != null) centerPhaseText.gameObject.SetActive(false);
        }

        public void ShowResultPopup(int scoreP1, int scoreP2)
        {
            HideCenterText();

            if (popupBackground != null) popupBackground.SetActive(true);

            if (scoreP1 > scoreP2)
            {
                if (p1WinPanel != null) p1WinPanel.SetActive(true);
                if (p1PtsText != null) p1PtsText.text = scoreP1.ToString();
            }
            else if (scoreP2 > scoreP1)
            {
                if (p2WinPanel != null) p2WinPanel.SetActive(true);
                if (p2PtsText != null) p2PtsText.text = scoreP2.ToString();
            }
            else
            {
                if (drawWinPanel != null) drawWinPanel.SetActive(true);
                if (drawPtsText != null) drawPtsText.text = scoreP1.ToString();
            }
        }

        private void GoToNextGame()
        {
            SceneFlowManager.Instance.GoTo(nextSceneName);
        }
    }
}