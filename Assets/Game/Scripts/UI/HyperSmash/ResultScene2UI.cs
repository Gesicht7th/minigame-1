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
        [SerializeField] private Button nextGameButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        [Header("── Next Scene ──────────────────────────────")]
        [SerializeField] private string nextSceneName = "LoadingScene";
        [SerializeField] private string nextGameSceneName = "Game3"; // Target setelah loading

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
            if (nextGameButton != null)
                nextGameButton.onClick.AddListener(() =>
                {
                    // Set target scene di loading scene
                    PlayerPrefs.SetString("NextGameScene", nextGameSceneName);
                    SceneManager.LoadScene(nextSceneName);
                });

            if (retryButton != null)
                retryButton.onClick.AddListener(() =>
                    SceneManager.LoadScene("HyperSmash"));

            if (menuButton != null)
                menuButton.onClick.AddListener(() =>
                    SceneManager.LoadScene("MainMenu"));
        }
    }
}