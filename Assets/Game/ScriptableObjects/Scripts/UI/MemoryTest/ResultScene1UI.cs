// Assets/_Game/Scripts/UI/ResultScene1UI.cs

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ResultScene1UI : MonoBehaviour
    {
        [Header("── Stats ────────────────────────────────")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI correctText;
        [SerializeField] private TextMeshProUGUI wrongText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject newHighBadge;

        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button continueButton; // → Game 2
        [SerializeField] private Button retryButton;    // → Ulangi Game 1

        [Header("── Scenes ──────────────────────────────")]
        [SerializeField] private string nextScene = "HyperSmash";
        [SerializeField] private string retryScene = "MemoryTest";

        void Start()
        {
            PopulateStats();

            // Ganti semua SceneManager.LoadScene dengan SceneFlowManager:

            // [PERBAIKAN] Penambahan kurung kurawal & penyesuaian fungsi GoToViaLoading
            continueButton?.onClick.AddListener(() =>
            {
                SceneFlowManager.Instance.GoTo(SceneNames.HyperSmash);
            });

            retryButton?.onClick.AddListener(() =>
            {
                GameDataBridge.Instance?.ClearGame1Score();
                SceneFlowManager.Instance.GoTo(SceneNames.MemoryTest);
            });
        }

        void PopulateStats()
        {
            var b = GameDataBridge.Instance;
            if (b == null) return;

            int score = b.MemoryTestScore;
            int correct = b.MemoryTestCorrect;
            int wrong = b.MemoryTestWrong;
            int total = correct + wrong;
            float accuracy = total > 0 ? (float)correct / total * 100f : 0f;

            if (scoreText) scoreText.text = score.ToString();
            if (correctText) correctText.text = correct.ToString();
            if (wrongText) wrongText.text = wrong.ToString();
            if (accuracyText) accuracyText.text = $"{accuracy:F0}%";

            int hs = PlayerPrefs.GetInt("MemoryTest_HighScore", 0);
            if (highScoreText) highScoreText.text = $"BEST: {hs}";
            newHighBadge?.SetActive(score >= hs && score > 0);
        }
    }
}