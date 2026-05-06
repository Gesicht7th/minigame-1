// Assets/_Game/Scripts/ReflexShowdown/ReflexScoreManager.cs

using System;
using UnityEngine;

namespace WizardPunk.Reflex
{
    public class ReflexScoreManager : MonoBehaviour
    {
        public static ReflexScoreManager Instance { get; private set; }

        [SerializeField] private ReflexConfig config;

        // ── Stats ─────────────────────────────────────
        public int P1RoundWins { get; private set; }
        public int P2RoundWins { get; private set; }
        public int P1TotalPoints { get; private set; }
        public int P2TotalPoints { get; private set; }
        public int RoundsPlayed { get; private set; }

        public event Action<int, int> OnScoreUpdated; // (p1wins, p2wins)

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Reset()
        {
            P1RoundWins = P2RoundWins = 0;
            P1TotalPoints = P2TotalPoints = 0;
            RoundsPlayed = 0;
        }

        /// <summary>winner: 1 = P1 menang, 2 = P2 menang, 0 = draw</summary>
        public void RecordRoundResult(int winner)
        {
            RoundsPlayed++;

            if (winner == 1)
            {
                P1RoundWins++;
                P1TotalPoints += config.pointsPerRoundWin;
            }
            else if (winner == 2)
            {
                P2RoundWins++;
                P2TotalPoints += config.pointsPerRoundWin;
            }
            // Draw: tidak ada poin

            OnScoreUpdated?.Invoke(P1RoundWins, P2RoundWins);
            Debug.Log($"[Score] Round winner: P{winner} | P1:{P1RoundWins} P2:{P2RoundWins}");
        }

        /// <summary>Siapa yang menang game keseluruhan? (1, 2, atau 0 = draw)</summary>
        public int GetGameWinner()
        {
            if (P1RoundWins > P2RoundWins) return 1;
            if (P2RoundWins > P1RoundWins) return 2;
            return 0;
        }

        /// <summary>Hitung total poin final termasuk bonus</summary>
        public void FinalizeScore()
        {
            int winner = GetGameWinner();
            if (winner == 1) P1TotalPoints += config.gameWinBonus;
            else if (winner == 2) P2TotalPoints += config.gameWinBonus;
        }

        public int GetHighScore(int player) =>
            PlayerPrefs.GetInt($"Reflex_P{player}_HighScore", 0);

        public void TrySaveHighScores()
        {
            if (P1TotalPoints > GetHighScore(1))
            {
                PlayerPrefs.SetInt("Reflex_P1_HighScore", P1TotalPoints);
                PlayerPrefs.Save();
            }
            if (P2TotalPoints > GetHighScore(2))
            {
                PlayerPrefs.SetInt("Reflex_P2_HighScore", P2TotalPoints);
                PlayerPrefs.Save();
            }
        }
    }
}