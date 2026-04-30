// Assets/_Game/Scripts/SceneFlow/GameDataBridge.cs

using UnityEngine;

namespace WizardPunk
{
    /// <summary>
    /// Menyimpan data antar scene menggunakan DontDestroyOnLoad.
    /// Menampung data untuk HyperSmash dan MemoryTest.
    /// </summary>
    public class GameDataBridge : MonoBehaviour
    {
        #region Singleton
        public static GameDataBridge Instance { get; private set; }
        #endregion

        #region Data - HyperSmash
        public HyperSmash.HyperSmashResult HyperSmashResult { get; private set; }
        public bool HasHyperSmashResult { get; private set; } = false;
        public int HyperSmashScore { get; set; } = 0;
        #endregion

        #region Data - MemoryTest (Game 1)
        // Properti ini diperlukan oleh ResultScene1UI dan MemoryTestGameManager
        public int MemoryTestScore { get; set; } = 0;
        public int MemoryTestCorrect { get; set; } = 0;
        public int MemoryTestWrong { get; set; } = 0;
        #endregion

        #region Data - Global
        public int Game3Score { get; set; } = 0;
        #endregion

        // ── Game 3: Reflex Showdown ────────────────────────
        #region Data - Game 3: Reflex Showdown
        public int ReflexP1Points { get; private set; }
        public int ReflexP2Points { get; private set; }
        public int ReflexP1Wins { get; private set; }
        public int ReflexP2Wins { get; private set; }
        public int ReflexWinner { get; private set; } // 1, 2, atau 0
        public bool HasReflexResult { get; private set; }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // Tetap hidup saat pindah scene
        }
        #endregion

        #region Save/Load - HyperSmash
        public void SaveHyperSmashResult(HyperSmash.HyperSmashResult result)
        {
            HyperSmashResult = result;
            HyperSmashScore = result.finalScore;
            HasHyperSmashResult = true;
            Debug.Log($"[Bridge] HyperSmash result saved. Score: {result.finalScore}");
        }

        public void ClearHyperSmashResult()
        {
            HasHyperSmashResult = false;
        }
        #endregion

        #region Save/Load - MemoryTest (Game 1)
        /// <summary>
        /// Dipanggil oleh MemoryTestGameManager saat game berakhir.
        /// </summary>
        public void SaveMemoryTestResult(int score, int correct, int wrong)
        {
            MemoryTestScore = score;
            MemoryTestCorrect = correct;
            MemoryTestWrong = wrong;
            Debug.Log($"[Bridge] MemoryTest result saved. Score: {score}");
        }

        /// <summary>
        /// Dipanggil oleh ResultScene1UI atau saat reset game.
        /// </summary>
        public void ClearGame1Score()
        {
            MemoryTestScore = 0;
            MemoryTestCorrect = 0;
            MemoryTestWrong = 0;
        }
        #endregion

        #region Save/Load - Game 3: Reflex Showdown
        public void SaveReflexResult(int p1Points, int p2Points,
                                      int p1Wins, int p2Wins, int winner)
        {
            ReflexP1Points = p1Points;
            ReflexP2Points = p2Points;
            ReflexP1Wins = p1Wins;
            ReflexP2Wins = p2Wins;
            ReflexWinner = winner;
            HasReflexResult = true;

            // Juga simpan ke HyperSmashScore / Game3Score untuk end screen
            Game3Score = p1Points; // atau bisa pakai field tersendiri
            Debug.Log($"[Bridge] Reflex saved: P1={p1Points} P2={p2Points} Winner=P{winner}");
        }
        #endregion
    }
}