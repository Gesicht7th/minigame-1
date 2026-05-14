// Assets/_Game/Scripts/ReflexShowdown/ReflexScoreManager.cs

using System;
using UnityEngine;

namespace WizardPunk.Reflex
{
    public class ReflexScoreManager : MonoBehaviour
    {
        public static ReflexScoreManager Instance { get; private set; }

        // ── Stats (Hearts System) ─────────────────────
        public int P1Hearts { get; private set; }
        public int P2Hearts { get; private set; }
        public int RoundsPlayed { get; private set; }

        public event Action<int, int> OnHeartsUpdated; // (p1Hearts, p2Hearts)

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ResetHearts()
        {
            P1Hearts = 3;
            P2Hearts = 3;
            RoundsPlayed = 0;
            OnHeartsUpdated?.Invoke(P1Hearts, P2Hearts);
        }

        /// <summary>winner: 1 = P1 menang (P2 hilang nyawa), 2 = P2 menang (P1 hilang nyawa), 0 = draw</summary>
        public void RecordRoundResult(int winner)
        {
            RoundsPlayed++;

            if (winner == 1)
            {
                P2Hearts--;
            }
            else if (winner == 2)
            {
                P1Hearts--;
            }
            // Draw: tidak ada yang hilang nyawa

            OnHeartsUpdated?.Invoke(P1Hearts, P2Hearts);
            Debug.Log($"[Score] Round winner: P{winner} | P1 Hearts:{P1Hearts} P2 Hearts:{P2Hearts}");
        }

        /// <summary>Siapa yang menang game keseluruhan? (1, 2, atau 0 = draw/masih main)</summary>
        public int GetGameWinner()
        {
            if (P2Hearts <= 0) return 1;
            if (P1Hearts <= 0) return 2;
            return 0;
        }
    }
}