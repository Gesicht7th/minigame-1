// Assets/_Game/Scripts/MemoryTest/MemoryTestConfig.cs
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    [CreateAssetMenu(
        fileName = "MemoryTestConfig",
        menuName = "WizardPunk/MemoryTest Config",
        order = 1
    )]
    public class MemoryTestConfig : ScriptableObject
    {
        [Header("── [NEW] Simultaneous 2-Player Match ─────")]
        public int totalRounds = 5;
        public float baseShowDelay = 0.8f;
        public float minShowDelay = 0.2f;
        public float delayDecay = 0.1f;
        public float memorizationTime = 1.5f;

        [Header("── [CORE] Round System ───────────────────")]
        public int roundsPerGame = 3;
        public float roundDurationSeconds = 45f;
        public float interRoundBreakDuration = 3f;

        [Header("── [CORE] Rune Memory ────────────────────")]
        public float runeShowDuration = 4f;
        public float runeInputDuration = 3f;

        [Header("── [CORE] Pattern Timing ─────────────────")]
        public float initialShowDuration = 2.5f;
        public float minimumShowDuration = 0.4f;
        public float showDurationDecay = 0.06f;

        [Header("── [CORE] Input Window ───────────────────")]
        public float initialInputWindow = 2.0f;
        public float minimumInputWindow = 0.4f;
        public float inputWindowDecay = 0.04f;

        [Header("── Scoring ───────────────────────────────")]
        public int correctPoints = 1;
        public int wrongPoints = -1;
        public int minimumScore = 0;

        [Header("── [CORE] Countdown ──────────────────────")]
        public int countdownStart = 3;
        public float countdownStepDuration = 1f;

        [Header("── [CORE] Anti Repeat ────────────────────")]
        [Range(1, 3)]
        public int antiRepeatBuffer = 2;

        // Computed helpers yang dibutuhkan oleh Core Scripts (JANGAN DIHAPUS)
        public float GetShowDuration(int patternIndex)
        {
            float d = initialShowDuration - patternIndex * showDurationDecay;
            return Mathf.Max(d, minimumShowDuration);
        }

        public float GetInputWindow(int patternIndex)
        {
            float w = initialInputWindow - patternIndex * inputWindowDecay;
            return Mathf.Max(w, minimumInputWindow);
        }
    }
}