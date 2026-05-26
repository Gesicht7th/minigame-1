// Assets/_Game/Scripts/MemoryTest/MemoryTestUIManager.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestUIManager : MonoBehaviour
    {
        // ============================================================
        // SINGLETON
        // ============================================================

        public static MemoryTestUIManager Instance
        {
            get;
            private set;
        }

        // ============================================================
        // HUD SCORE
        // ============================================================

        [Header("HUD Score")]

        [SerializeField]
        private TextMeshProUGUI scoreP1Text;

        [SerializeField]
        private TextMeshProUGUI scoreP2Text;

        // ============================================================
        // HUD CENTER & DIFFICULTY
        // ============================================================

        [Header("HUD Center & Difficulty")]

        [SerializeField]
        private TextMeshProUGUI centerPhaseText;

        [SerializeField]
        private TextMeshProUGUI difficultyText;

        // ============================================================
        // HUD TIMER
        // ============================================================

        [Header("HUD Timer")]

        [SerializeField]
        private TextMeshProUGUI timerText;

        [SerializeField]
        private Image timerFillBar;

        // ============================================================
        // TUTORIAL
        // ============================================================

        [Header("── Tutorial ──")]

        [SerializeField]
        private GameObject tutorialPanel;

        [SerializeField]
        private Button tutorialGoButton;

        // ============================================================
        // TIMES UP
        // ============================================================

        [Header("── Times Up Pop-Up ──")]

        [SerializeField]
        private GameObject timesUpPanel;

        // ============================================================
        // RESULT POPUP
        // ============================================================

        [Header("── Result Pop-Up ──")]

        [SerializeField]
        private GameObject popupBackground;

        [SerializeField]
        private GameObject p1WinPanel;

        [SerializeField]
        private GameObject p2WinPanel;

        [SerializeField]
        private GameObject drawWinPanel;

        // ============================================================
        // RESULT TEXT
        // ============================================================

        [SerializeField]
        private TextMeshProUGUI p1PtsText;

        [SerializeField]
        private TextMeshProUGUI p2PtsText;

        [SerializeField]
        private TextMeshProUGUI drawPtsText;

        // ============================================================
        // RESULT BUTTONS
        // ============================================================

        [SerializeField]
        private Button p1NextButton;

        [SerializeField]
        private Button p2NextButton;

        [SerializeField]
        private Button drawNextButton;

        // ============================================================
        // SCENE FLOW
        // ============================================================

        [SerializeField]
        private string nextSceneName =
            "test hypersmash";

        // ============================================================
        // STATE
        // ============================================================

        public bool IsTutorialDone
        {
            get;
            private set;
        }

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // ========================================================
            // SCORE EVENT
            // ========================================================

            if (MemoryTestScoreManager.Instance != null)
            {
                MemoryTestScoreManager.Instance
                    .OnScoreUpdated += UpdateScoreUI;
            }

            // ========================================================
            // INITIAL UI STATE
            // ========================================================

            if (popupBackground != null)
            {
                popupBackground.SetActive(false);
            }

            if (timesUpPanel != null)
            {
                timesUpPanel.SetActive(false);
            }

            if (p1WinPanel != null)
            {
                p1WinPanel.SetActive(false);
            }

            if (p2WinPanel != null)
            {
                p2WinPanel.SetActive(false);
            }

            if (drawWinPanel != null)
            {
                drawWinPanel.SetActive(false);
            }

            // ========================================================
            // NEXT BUTTONS
            // ========================================================

            if (p1NextButton != null)
            {
                p1NextButton.onClick
                    .AddListener(GoToNextGame);
            }

            if (p2NextButton != null)
            {
                p2NextButton.onClick
                    .AddListener(GoToNextGame);
            }

            if (drawNextButton != null)
            {
                drawNextButton.onClick
                    .AddListener(GoToNextGame);
            }

            // ========================================================
            // TUTORIAL GO BUTTON
            // ========================================================

            if (tutorialGoButton != null)
            {
                tutorialGoButton.onClick
                    .AddListener(() =>
                    {
                        IsTutorialDone = true;
                    });
            }
        }

        private void OnDestroy()
        {
            if (MemoryTestScoreManager.Instance != null)
            {
                MemoryTestScoreManager.Instance
                    .OnScoreUpdated -= UpdateScoreUI;
            }
        }

        // ============================================================
        // TUTORIAL
        // ============================================================

        public void ShowTutorial()
        {
            IsTutorialDone = false;

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
            }
        }

        public void HideTutorial()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }

        public void TriggerTutorialDone()
        {
            IsTutorialDone = true;
        }

        // ============================================================
        // TIMES UP
        // ============================================================

        public void ShowTimesUp()
        {
            if (timesUpPanel != null)
            {
                timesUpPanel.SetActive(true);
            }
        }

        public void HideTimesUp()
        {
            if (timesUpPanel != null)
            {
                timesUpPanel.SetActive(false);
            }
        }

        // ============================================================
        // SCORE UI
        // ============================================================

        private void UpdateScoreUI(
            int playerId,
            int score)
        {
            if (playerId == 1 &&
                scoreP1Text != null)
            {
                scoreP1Text.text =
                    score.ToString();
            }

            if (playerId == 2 &&
                scoreP2Text != null)
            {
                scoreP2Text.text =
                    score.ToString();
            }
        }

        // ============================================================
        // DIFFICULTY
        // ============================================================

        public void UpdateDifficultyText(
            string diff)
        {
            if (difficultyText != null)
            {
                difficultyText.text = diff;
            }
        }

        // ============================================================
        // TIMER
        // ============================================================

        public void UpdateTimerUI(
            float currentTime,
            float maxTime)
        {
            if (timerText != null)
            {
                timerText.text =
                    $"{Mathf.CeilToInt(currentTime)}s";
            }

            if (timerFillBar != null)
            {
                timerFillBar.fillAmount =
                    currentTime / maxTime;
            }
        }

        // ============================================================
        // CENTER TEXT
        // ============================================================

        public void ShowCenterText(string msg)
        {
            if (centerPhaseText != null)
            {
                centerPhaseText.text = msg;

                centerPhaseText.gameObject
                    .SetActive(true);
            }
        }

        public void HideCenterText()
        {
            if (centerPhaseText != null)
            {
                centerPhaseText.gameObject
                    .SetActive(false);
            }
        }

        // ============================================================
        // RESULT POPUP
        // ============================================================

        public void ShowResultPopup(
            int scoreP1,
            int scoreP2)
        {
            HideCenterText();

            // ========================================================
            // SHOW VIRTUAL POINTER
            // ========================================================

            CursorGlobal.ShowVirtualCursor();

            // ========================================================
            // POPUP BACKGROUND
            // ========================================================

            if (popupBackground != null)
            {
                popupBackground.SetActive(true);
            }

            // ========================================================
            // RESULT LOGIC
            // ========================================================

            if (scoreP1 > scoreP2)
            {
                if (p1WinPanel != null)
                {
                    p1WinPanel.SetActive(true);
                }

                if (p1PtsText != null)
                {
                    p1PtsText.text =
                        scoreP1.ToString();
                }
            }
            else if (scoreP2 > scoreP1)
            {
                if (p2WinPanel != null)
                {
                    p2WinPanel.SetActive(true);
                }

                if (p2PtsText != null)
                {
                    p2PtsText.text =
                        scoreP2.ToString();
                }
            }
            else
            {
                if (drawWinPanel != null)
                {
                    drawWinPanel.SetActive(true);
                }

                if (drawPtsText != null)
                {
                    drawPtsText.text =
                        scoreP1.ToString();
                }
            }
        }

        // ============================================================
        // NEXT GAME
        // ============================================================

        private void GoToNextGame()
        {
            SceneFlowManager.Instance
                .GoTo(nextSceneName);
        }
    }
}