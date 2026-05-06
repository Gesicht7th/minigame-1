// Assets/_Game/Scripts/MemoryTest/MemoryTestScoreManager.cs
using System;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestScoreManager : MonoBehaviour
    {
        public static MemoryTestScoreManager Instance { get; private set; }

        [SerializeField] private MemoryTestConfig config;

        public int ScoreP1 { get; private set; }
        public int ScoreP2 { get; private set; }

        public event Action<int, int> OnScoreUpdated; // (PlayerID, NewScore)

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ResetScores()
        {
            ScoreP1 = 0;
            ScoreP2 = 0;
            OnScoreUpdated?.Invoke(1, ScoreP1);
            OnScoreUpdated?.Invoke(2, ScoreP2);
        }

        public void ApplyScore(int playerId, bool isCorrect)
        {
            int delta = isCorrect ? config.correctPoints : config.wrongPoints;
            if (playerId == 1) ScoreP1 = Mathf.Max(0, ScoreP1 + delta);
            else ScoreP2 = Mathf.Max(0, ScoreP2 + delta);

            OnScoreUpdated?.Invoke(playerId, (playerId == 1 ? ScoreP1 : ScoreP2));
        }
    }
}