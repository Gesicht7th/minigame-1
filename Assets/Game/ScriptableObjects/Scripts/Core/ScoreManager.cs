// Assets/_Game/Scripts/Core/ScoreManager.cs

using System;
using WizardPunk.MemoryTest;
using UnityEngine;

namespace WizardPunk
{
    public class ScoreManager : MonoBehaviour
    {
        #region Singleton
        public static ScoreManager Instance { get; private set; }
        #endregion

        #region Public Properties
        public int CurrentScore { get; private set; } = 0;
        public int CorrectCount { get; private set; } = 0;
        public int WrongCount { get; private set; } = 0;
        public int TimeoutCount { get; private set; } = 0;
        public int TotalRounds => CorrectCount + WrongCount + TimeoutCount;
        #endregion

        #region Events
        /// <summary>Fired saat skor berubah. (newScore, delta, result)</summary>
        public event Action<int, int, RoundResult> OnScoreChanged;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        #endregion

        #region Public Methods
        public void Reset()
        {
            CurrentScore = 0;
            CorrectCount = 0;
            WrongCount = 0;
            TimeoutCount = 0;
            Debug.Log("[Score] Reset");
        }

        public void ApplyResult(RoundResult result, MemoryTestConfig config)
        {
            int delta = 0;

            switch (result)
            {
                case RoundResult.Correct:
                    delta = config.correctPoints;
                    CorrectCount++;
                    break;
                case RoundResult.Wrong:
                    delta = config.wrongPoints;
                    WrongCount++;
                    break;
                case RoundResult.Timeout:
                    delta = config.wrongPoints; // Timeout juga kena penalti
                    TimeoutCount++;
                    break;
            }

            CurrentScore = Mathf.Max(config.minimumScore, CurrentScore + delta);
            OnScoreChanged?.Invoke(CurrentScore, delta, result);

            string icon = result == RoundResult.Correct ? "✅" : "❌";
            Debug.Log($"[Score] {icon} {result} | Delta: {delta:+0;-0} | Total: {CurrentScore}");
        }

        public float GetAccuracy()
        {
            if (TotalRounds == 0) return 0f;
            return (float)CorrectCount / TotalRounds * 100f;
        }

        public int GetHighScore()
        {
            return PlayerPrefs.GetInt("MemoryTest_HighScore", 0);
        }

        public bool TrySaveHighScore()
        {
            int current = GetHighScore();
            if (CurrentScore > current)
            {
                PlayerPrefs.SetInt("MemoryTest_HighScore", CurrentScore);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
        #endregion
    }
}