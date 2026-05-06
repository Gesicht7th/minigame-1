// Assets/_Game/Scripts/UI/ResultScene3UI.cs

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ResultScene3UI : MonoBehaviour
    {
        [Header("── Stats ────────────────────────────────")]
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI p1WinsText;
        [SerializeField] private TextMeshProUGUI p2WinsText;
        [SerializeField] private TextMeshProUGUI p1PointsText;
        [SerializeField] private TextMeshProUGUI p2PointsText;

        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button continueButton; // → End Screen
        [SerializeField] private Button retryButton;    // → Reflex lagi

        [Header("── Scenes ──────────────────────────────")]
        [SerializeField] private string nextScene = "EndScreen";
        [SerializeField] private string retryScene = "ReflexShowdown";

        void Start()
        {
            PopulateStats();
            continueButton?.onClick.AddListener(() =>
    SceneFlowManager.Instance.GoTo(SceneNames.EndScreen));

            retryButton?.onClick.AddListener(() =>
                SceneFlowManager.Instance.GoTo(SceneNames.ReflexShowdown));
        }

        void PopulateStats()
        {
            var b = GameDataBridge.Instance;
            if (b == null) return;

            int w = b.ReflexWinner;
            if (winnerText != null)
                winnerText.text = w == 0 ? "MATCH DRAWN!"
                    : $"PLAYER {w} WINS THE MATCH!";

            if (p1WinsText != null) p1WinsText.text = $"P1 Rounds Won: {b.ReflexP1Wins}";
            if (p2WinsText != null) p2WinsText.text = $"P2 Rounds Won: {b.ReflexP2Wins}";
            if (p1PointsText != null) p1PointsText.text = $"P1 Points: {b.ReflexP1Points}";
            if (p2PointsText != null) p2PointsText.text = $"P2 Points: {b.ReflexP2Points}";
        }
    }
}