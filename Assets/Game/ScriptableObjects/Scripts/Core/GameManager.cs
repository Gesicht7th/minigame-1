// Assets/_Game/Scripts/Gameplay/PatternManager.cs
// Mengelola urutan pola arah yang ditampilkan ke player

using System.Collections.Generic;
using WizardPunk.MemoryTest;
using UnityEngine;

namespace WizardPunk
{
    public class PatternManager : MonoBehaviour
    {
        #region Singleton
        public static PatternManager Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("Configuration")]
        [SerializeField] private MemoryTestConfig config;
        #endregion

        #region Public Properties
        public MemoryTestConfig Config => config;
        public WandDirection CurrentTarget { get; private set; } = WandDirection.None;
        public int RoundNumber { get; private set; } = 0;
        public float CurrentShowDuration => config.GetShowDuration(RoundNumber);
        public float CurrentInputWindow => config.GetInputWindow(RoundNumber);
        #endregion

        #region Private Fields
        private static readonly WandDirection[] ALL_DIRECTIONS = {
            WandDirection.Up,
            WandDirection.Down,
            WandDirection.Left,
            WandDirection.Right
        };

        private Queue<WandDirection> recentQueue = new Queue<WandDirection>();
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            RoundNumber = 0;
            CurrentTarget = WandDirection.None;
            recentQueue.Clear();
            Debug.Log("[PatternManager] Initialized");
        }

        /// <summary>
        /// Generate pola berikutnya dan increment ronde.
        /// Returns arah target yang harus diikuti player.
        /// </summary>
        public WandDirection GenerateNext()
        {
            RoundNumber++;
            WandDirection next = PickDirection();
            CurrentTarget = next;

            // Update anti-repeat queue
            recentQueue.Enqueue(next);
            if (recentQueue.Count > config.antiRepeatBuffer)
                recentQueue.Dequeue();

            Debug.Log($"[Pattern] Round {RoundNumber} | Target: {next} | " +
                      $"ShowDur: {CurrentShowDuration:F2}s | InputWin: {CurrentInputWindow:F2}s");

            return next;
        }

        public string GetTimingReport()
        {
            return $"Round {RoundNumber}: Show={CurrentShowDuration:F2}s, Input={CurrentInputWindow:F2}s";
        }
        #endregion

        #region Direction Generation
        private WandDirection PickDirection()
        {
            // Kumpulkan candidates yang tidak ada di recent queue
            List<WandDirection> candidates = new List<WandDirection>();
            foreach (var dir in ALL_DIRECTIONS)
            {
                if (!recentQueue.Contains(dir))
                    candidates.Add(dir);
            }

            // Jika semua arah masuk buffer (buffer terlalu besar), ambil semua
            if (candidates.Count == 0)
                candidates.AddRange(ALL_DIRECTIONS);

            // Pilih acak dari candidates
            return candidates[Random.Range(0, candidates.Count)];
        }
        #endregion
    }
}