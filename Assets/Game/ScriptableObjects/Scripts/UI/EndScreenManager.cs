// Assets/_Game/Scripts/UI/EndScreenManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class EndScreenManager : MonoBehaviour
    {
        [Header("── Bonus Skor Game 3 ──")]
        [Tooltip("Poin yang diberikan kepada pemenang Game 3 (Duel Nyawa)")]
        [SerializeField] private int game3WinBonus = 50;

        [Header("── Panels ──")]
        [SerializeField] private GameObject p1WinPanel; // Panel Fura Menang
        [SerializeField] private GameObject p2WinPanel; // Panel Oura Menang

        [Header("── UI Panel Fura (P1) ──")]
        [SerializeField] private TextMeshProUGUI p1Win_P1ScoreText; // Skor kiri
        [SerializeField] private TextMeshProUGUI p1Win_P2ScoreText; // Skor kanan
        [SerializeField] private Button p1Win_HomeButton;           // Tombol panah kembali

        [Header("── UI Panel Oura (P2) ──")]
        [SerializeField] private TextMeshProUGUI p2Win_P1ScoreText; // Skor kiri
        [SerializeField] private TextMeshProUGUI p2Win_P2ScoreText; // Skor kanan
        [SerializeField] private Button p2Win_HomeButton;           // Tombol panah kembali

        [Header("── Scene Navigation ──")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        void Start()
        {
            // Memastikan kursor muncul untuk klik tombol
            DebugInputManager.ApplyCursorMode();

            // 1. Ambil Data Skor dari Memory (PlayerPrefs)
            int g1_p1 = PlayerPrefs.GetInt("G1_ScoreP1", 0);
            int g1_p2 = PlayerPrefs.GetInt("G1_ScoreP2", 0);

            int g2_p1 = PlayerPrefs.GetInt("G2_ScoreP1", 0);
            int g2_p2 = PlayerPrefs.GetInt("G2_ScoreP2", 0);

            int g3_winner = PlayerPrefs.GetInt("G3_Winner", 0); // 1 = Fura, 2 = Oura

            // 2. Kalkulasi Total Keseluruhan
            int totalP1 = g1_p1 + g2_p1 + (g3_winner == 1 ? game3WinBonus : 0);
            int totalP2 = g1_p2 + g2_p2 + (g3_winner == 2 ? game3WinBonus : 0);

            Debug.Log($"[EndScreen] Total P1: {totalP1} | Total P2: {totalP2}");

            // Sembunyikan kedua panel di awal
            if (p1WinPanel != null) p1WinPanel.SetActive(false);
            if (p2WinPanel != null) p2WinPanel.SetActive(false);

            // 3. Tentukan Siapa Pemenang Akhir
            if (totalP1 >= totalP2)
            {
                // Fura (P1) Menang (atau Seri)
                if (p1WinPanel != null) p1WinPanel.SetActive(true);
                if (p1Win_P1ScoreText != null) p1Win_P1ScoreText.text = totalP1.ToString();
                if (p1Win_P2ScoreText != null) p1Win_P2ScoreText.text = totalP2.ToString();
            }
            else
            {
                // Oura (P2) Menang
                if (p2WinPanel != null) p2WinPanel.SetActive(true);
                if (p2Win_P1ScoreText != null) p2Win_P1ScoreText.text = totalP1.ToString();
                if (p2Win_P2ScoreText != null) p2Win_P2ScoreText.text = totalP2.ToString();
            }

            // 4. Pasang Event Tombol Kembali
            if (p1Win_HomeButton != null) p1Win_HomeButton.onClick.AddListener(FinishGameAndGoHome);
            if (p2Win_HomeButton != null) p2Win_HomeButton.onClick.AddListener(FinishGameAndGoHome);
        }

        private void FinishGameAndGoHome()
        {
            // Reset atau bersihkan riwayat memori untuk turnamen berikutnya
            PlayerPrefs.DeleteKey("G1_ScoreP1");
            PlayerPrefs.DeleteKey("G1_ScoreP2");
            PlayerPrefs.DeleteKey("G2_ScoreP1");
            PlayerPrefs.DeleteKey("G2_ScoreP2");
            PlayerPrefs.DeleteKey("G3_Winner");
            PlayerPrefs.Save();

            // Pindah ke Main Menu
            if (SceneFlowManager.Instance != null)
                SceneFlowManager.Instance.GoTo(mainMenuSceneName);
        }
    }
}