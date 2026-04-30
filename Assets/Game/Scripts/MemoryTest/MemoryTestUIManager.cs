// Assets/_Game/Scripts/MemoryTest/MemoryTestUIManager.cs
using TMPro;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestUIManager : MonoBehaviour
    {
        public static MemoryTestUIManager Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI scoreP1Text;
        [SerializeField] private TextMeshProUGUI scoreP2Text;
        [SerializeField] private TextMeshProUGUI centerPhaseText; // Cth: "P1 MEMORIZE!"
        [SerializeField] private TextMeshProUGUI roundText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            MemoryTestScoreManager.Instance.OnScoreUpdated += UpdateScoreUI;
        }

        void OnDestroy()
        {
            if (MemoryTestScoreManager.Instance != null)
                MemoryTestScoreManager.Instance.OnScoreUpdated -= UpdateScoreUI;
        }

        private void UpdateScoreUI(int playerId, int score)
        {
            if (playerId == 1 && scoreP1Text != null) scoreP1Text.text = $"P1: {score}";
            if (playerId == 2 && scoreP2Text != null) scoreP2Text.text = $"P2: {score}";
        }

        public void UpdateRoundText(int round, int total)
        {
            if (roundText != null) roundText.text = $"ROUND {round}/{total}";
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
    }
}