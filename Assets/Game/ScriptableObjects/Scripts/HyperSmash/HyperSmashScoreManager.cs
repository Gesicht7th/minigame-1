// Assets/_Game/Scripts/HyperSmash/HyperSmashScoreManager.cs

using System;
using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class HyperSmashScoreManager : MonoBehaviour
    {
        #region Singleton
        public static HyperSmashScoreManager Instance { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired saat skor player berubah.
        /// Parameter: (PlayerIndex siapa, skor baru, delta/perubahan)
        /// </summary>
        public event Action<PlayerIndex, int, int> OnScoreChanged;
        #endregion

        #region Private State
        // Skor per player
        private int[] scores = new int[2];
        // Total tembakan per player
        private int[] shots = new int[2];
        // Total hit (mengenai kristal) per player
        private int[] hits = new int[2];
        // Jumlah crystal per tipe yang dihancurkan per player
        private int[,] crystalCount = new int[2, 4]; // [player, crystalType]
        // Berapa kali bomb kena per player
        private int[] bombsHit = new int[2];
        #endregion

        #region Public Properties — Score
        public int ScoreP1 => scores[(int)PlayerIndex.Player1];
        public int ScoreP2 => scores[(int)PlayerIndex.Player2];

        public int GetScore(PlayerIndex p) => scores[(int)p];
        #endregion

        #region Public Properties — Stats
        public int GetShots(PlayerIndex p) => shots[(int)p];
        public int GetHits(PlayerIndex p) => hits[(int)p];
        public int GetBombsHit(PlayerIndex p) => bombsHit[(int)p];

        public int GetCrystalCount(PlayerIndex p, CrystalType t)
            => crystalCount[(int)p, (int)t];

        public float GetAccuracy(PlayerIndex p)
        {
            int s = shots[(int)p];
            int h = hits[(int)p];
            if (s == 0) return 0f;
            return (float)h / s * 100f;
        }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        #endregion

        #region Public Methods
        /// <summary>Reset semua data (dipanggil saat game mulai)</summary>
        public void ResetAll()
        {
            for (int i = 0; i < 2; i++)
            {
                scores[i] = 0;
                shots[i] = 0;
                hits[i] = 0;
                bombsHit[i] = 0;
                for (int t = 0; t < 4; t++)
                    crystalCount[i, t] = 0;
            }
            Debug.Log("[HyperSmashScore] Reset semua player");
        }

        /// <summary>Tambah skor ke player tertentu</summary>
        public void AddScore(PlayerIndex player, int delta)
        {
            int idx = (int)player;
            scores[idx] = Mathf.Max(0, scores[idx] + delta);
            OnScoreChanged?.Invoke(player, scores[idx], delta);
            Debug.Log($"[Score] {player} | Delta: {delta:+0;-0} | Total: {scores[idx]}");
        }

        /// <summary>Catat bahwa player menembak</summary>
        public void RegisterShot(PlayerIndex player)
        {
            shots[(int)player]++;
        }

        /// <summary>Catat bahwa tembakan player mengenai kristal</summary>
        public void RegisterHit(PlayerIndex player)
        {
            hits[(int)player]++;
        }

        /// <summary>
        /// Catat bahwa player menghancurkan kristal tertentu.
        /// Skor ditambahkan otomatis dari Crystal.ScoreValue.
        /// </summary>
        public void RegisterCrystalKill(PlayerIndex player, Crystal crystal)
        {
            int pi = (int)player;
            int ti = (int)crystal.Type;

            if (crystal.Type == CrystalType.Bomb)
                bombsHit[pi]++;
            else
                crystalCount[pi, ti]++;

            AddScore(player, crystal.ScoreValue);
        }

        /// <summary>
        /// Simpan high score masing-masing player.
        /// Return true jika salah satu memecahkan rekor.
        /// </summary>
        public (bool p1IsNew, bool p2IsNew) TrySaveHighScores()
        {
            bool p1New = TrySave(PlayerIndex.Player1);
            bool p2New = TrySave(PlayerIndex.Player2);
            return (p1New, p2New);
        }

        public int GetHighScore(PlayerIndex player)
        {
            string key = player == PlayerIndex.Player1
                ? "HyperSmash_HighScore_P1"
                : "HyperSmash_HighScore_P2";
            return PlayerPrefs.GetInt(key, 0);
        }
        #endregion

        #region Private Helpers
        private bool TrySave(PlayerIndex player)
        {
            string key = player == PlayerIndex.Player1
                ? "HyperSmash_HighScore_P1"
                : "HyperSmash_HighScore_P2";

            int current = PlayerPrefs.GetInt(key, 0);
            int score = scores[(int)player];

            if (score > current)
            {
                PlayerPrefs.SetInt(key, score);
                PlayerPrefs.Save();
                Debug.Log($"[Score] 🏆 New High Score {player}: {score}");
                return true;
            }
            return false;
        }
        #endregion
    }
}