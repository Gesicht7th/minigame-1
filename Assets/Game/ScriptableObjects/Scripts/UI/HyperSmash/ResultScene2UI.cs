// Assets/_Game/Scripts/UI/HyperSmash/ResultScene2UI.cs

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// UI Result Scene setelah Hyper Smash.
    /// Menampilkan skor, statistik, dan tombol lanjut ke game berikutnya.
    /// </summary>
    public class ResultScene2UI : MonoBehaviour
    {
        [Header("── Text Fields ─────────────────────────────")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI blueCountText;
        [SerializeField] private TextMeshProUGUI purpleCountText;
        [SerializeField] private TextMeshProUGUI redCountText;
        [SerializeField] private TextMeshProUGUI bombHitText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject newHighScoreBadge;

        [Header("── Buttons ─────────────────────────────────")]
        [SerializeField] private Button nextGameButton; // Berperan sebagai continueButton dari Code 2
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        [Header("── Next Scene ──────────────────────────────")]
        [SerializeField] private string nextSceneName = "LoadingScene";
        [SerializeField] private string nextGameSceneName = "Game3";

        void Start()
        {
            PopulateUI();
            BindButtons();
        }

        private void PopulateUI()
        {
            var bridge = GameDataBridge.Instance;
            if (bridge == null || !bridge.HasHyperSmashResult)
            {
                Debug.LogWarning("[Result2] Tidak ada data hasil game!");
                return;
            }

            HyperSmashResult r = bridge.HyperSmashResult;

            if (finalScoreText) finalScoreText.text = r.finalScore.ToString("D4");
            if (blueCountText) blueCountText.text = r.blueDestroyed.ToString();
            if (purpleCountText) purpleCountText.text = r.purpleDestroyed.ToString();
            if (redCountText) redCountText.text = r.redDestroyed.ToString();
            if (bombHitText) bombHitText.text = r.bombsHit.ToString();
            if (accuracyText) accuracyText.text = $"{r.Accuracy:F0}%";

            int hs = PlayerPrefs.GetInt("HyperSmash_HighScore", 0);
            if (highScoreText) highScoreText.text = $"BEST: {hs}";

            bool isNew = r.finalScore >= hs;
            if (newHighScoreBadge) newHighScoreBadge.SetActive(isNew);
        }

        private void BindButtons()
        {
            // 1. Integrasi Code 2 (continueButton diubah targetnya ke nextGameButton)
            if (nextGameButton != null)
            {
                nextGameButton.onClick.AddListener(() =>
                    SceneFlowManager.Instance.GoToViaLoading(SceneNames.ReflexShowdown));
            }

            // 2. Integrasi Code 2 (Update retryButton dengan ClearGame2Score)
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(() =>
                {
                    // Sekarang fungsi ini bisa dipanggil dengan sukses
                    GameDataBridge.Instance?.ClearGame2Score();
                    SceneFlowManager.Instance.GoTo(SceneNames.HyperSmash);
                });
            }

            // 3. Mempertahankan Code 1 (Fungsi menuButton tetap utuh)
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(() =>
                    SceneManager.LoadScene("MainMenu"));
            }
        }
    }
}