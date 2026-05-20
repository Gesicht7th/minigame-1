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

        [Header("── Result Pop-Up ──")]
        [SerializeField] private GameObject popupBackground;    // <--- TAMBAHAN UNTUK BACKGROUND GELAP
        [SerializeField] private GameObject p1WinPanel;     // Panel Fura Oren
        [SerializeField] private GameObject p2WinPanel;     // Panel Oura Biru
        [SerializeField] private GameObject drawWinPanel;   // Panel Draw (Seri)

        [SerializeField] private TextMeshProUGUI p1PtsText; // Text Poin di panel Fura
        [SerializeField] private TextMeshProUGUI p2PtsText; // Text Poin di panel Oura
        [SerializeField] private TextMeshProUGUI drawPtsText; // Text Poin di panel Draw

        [SerializeField] private Button p1NextButton;       // Tombol Next di panel Fura
        [SerializeField] private Button p2NextButton;       // Tombol Next di panel Oura
        [SerializeField] private Button drawNextButton;     // Tombol Next di panel Draw

        [SerializeField] private string nextSceneName = "HyperSmash"; // Nama Scene game selanjutnya

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            MemoryTestScoreManager.Instance.OnScoreUpdated += UpdateScoreUI;

            // Matikan background gelap saat game baru mulai
            if (popupBackground != null) popupBackground.SetActive(false);

            if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
            if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
            if (drawNextButton != null) drawNextButton.onClick.AddListener(GoToNextGame);
        }

        void OnDestroy()
        {
            if (MemoryTestScoreManager.Instance != null)
                MemoryTestScoreManager.Instance.OnScoreUpdated -= UpdateScoreUI;
        }

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

            // Nyalakan background gelap
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