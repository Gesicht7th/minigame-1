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
        [SerializeField] private GameObject p1WinPanel;     // Panel Fura Oren
        [SerializeField] private GameObject p2WinPanel;     // Panel Oura Biru
        [SerializeField] private TextMeshProUGUI p1PtsText; // Text Poin di panel Fura
        [SerializeField] private TextMeshProUGUI p2PtsText; // Text Poin di panel Oura
        [SerializeField] private Button p1NextButton;       // Tombol Next di panel Fura
        [SerializeField] private Button p2NextButton;       // Tombol Next di panel Oura
        [SerializeField] private string nextSceneName = "HyperSmash"; // Nama Scene game selanjutnya

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            MemoryTestScoreManager.Instance.OnScoreUpdated += UpdateScoreUI;

            // Memasang fungsi pada tombol Next
            if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
            if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
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

        // --- FUNGSI BARU: Memunculkan Pop Up Sesuai Pemenang ---
        public void ShowResultPopup(int scoreP1, int scoreP2)
        {
            // Sembunyikan center text agar layar bersih
            HideCenterText();

            // P1 Menang (Fura)
            if (scoreP1 > scoreP2)
            {
                if (p1WinPanel != null) p1WinPanel.SetActive(true);
                if (p1PtsText != null) p1PtsText.text = scoreP1.ToString();
            }
            // P2 Menang (Oura)
            else if (scoreP2 > scoreP1)
            {
                if (p2WinPanel != null) p2WinPanel.SetActive(true);
                if (p2PtsText != null) p2PtsText.text = scoreP2.ToString();
            }
            // Seri (Draw) - Memunculkan panel P1 tapi teksnya diberi keterangan DRAW
            else
            {
                if (p1WinPanel != null) p1WinPanel.SetActive(true);
                if (p1PtsText != null) p1PtsText.text = scoreP1.ToString() + "\n(DRAW)";
            }
        }

        // Fungsi untuk pindah scene saat tombol NEXT diklik
        private void GoToNextGame()
        {
            Debug.Log("Melanjutkan ke game berikutnya: " + nextSceneName);
            SceneFlowManager.Instance.GoTo(nextSceneName);
        }
    }
}