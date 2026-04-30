// Assets/_Game/Scripts/MemoryTest/MemoryTestPatternManager.cs

using System.Collections.Generic;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    // Reuse enum WandDirection dari namespace WizardPunk
    // Up, Down, Left, Right, None

    public class MemoryTestPatternManager : MonoBehaviour
    {
        public static MemoryTestPatternManager Instance { get; private set; }

        [SerializeField] private MemoryTestConfig config;

        public WandDirection CurrentTarget { get; private set; }
        public int PatternIndex { get; private set; }

        public float CurrentShowDuration => config.GetShowDuration(PatternIndex);
        public float CurrentInputWindow => config.GetInputWindow(PatternIndex);

        private static readonly WandDirection[] ALL_DIRS =
        {
            WandDirection.Up, WandDirection.Down,
            WandDirection.Left, WandDirection.Right
        };

        private Queue<WandDirection> recentQueue = new Queue<WandDirection>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize()
        {
            PatternIndex = 0;
            CurrentTarget = WandDirection.None;
            recentQueue.Clear();
        }

        public WandDirection GenerateNext()
        {
            var candidates = new List<WandDirection>();
            foreach (var d in ALL_DIRS)
                if (!recentQueue.Contains(d)) candidates.Add(d);

            if (candidates.Count == 0) candidates.AddRange(ALL_DIRS);

            WandDirection next = candidates[Random.Range(0, candidates.Count)];
            CurrentTarget = next;

            recentQueue.Enqueue(next);
            if (recentQueue.Count > config.antiRepeatBuffer)
                recentQueue.Dequeue();

            PatternIndex++;
            Debug.Log($"[Pattern] #{PatternIndex} Target={next} " +
                      $"Show={CurrentShowDuration:F2}s Input={CurrentInputWindow:F2}s");
            return next;
        }
    }
}