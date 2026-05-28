// Assets/_Game/Scripts/MemoryTest/MemoryTestGameManager.cs
using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestGameManager : MonoBehaviour
    {
        [SerializeField] private MemoryTestConfig config;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private MemoryTestUIManager uiManager;
        [SerializeField] private MemoryTestScoreManager scoreManager;

        [Header("── Animators ──────────────────────")]
        [SerializeField] private Animator p1Animator;
        [SerializeField] private Animator p2Animator;

        [Header("── Wand Inputs ──────────────────────")]
        [SerializeField] private WandSerialReader p1SerialReader;
        [SerializeField] private WandSerialReader p2SerialReader;

        private int p1InputIdx, p2InputIdx;
        private bool isInputPhase;

        private int currentActiveRunes;
        private readonly string[] diffNames = { "EASY", "MEDIUM", "HARD", "EXPERT" };
        private readonly int[] runeCounts = { 3, 4, 5, 6 };
        private readonly float[] roundTimers = { 10f, 7f, 5f, 4f };

        void Start()
        {
            if (p1SerialReader == null) p1SerialReader = null;
            if (p2SerialReader == null) p2SerialReader = null;

            WandSerialReader[] readers = FindObjectsOfType<WandSerialReader>();

            foreach (var reader in readers)
            {
                if (reader.name.ToUpper().Contains("P1") || reader.name.ToUpper().Contains("PLAYER1"))
                {
                    if (p1SerialReader == null) p1SerialReader = reader;
                }
                else if (reader.name.ToUpper().Contains("P2") || reader.name.ToUpper().Contains("PLAYER2"))
                {
                    if (p2SerialReader == null) p2SerialReader = reader;
                }
            }

            if (p1SerialReader == null && readers.Length > 0) p1SerialReader = readers[0];
            if (p2SerialReader == null && readers.Length > 1) p2SerialReader = (readers[0] == p1SerialReader) ? readers[1] : readers[0];

            if (p1SerialReader == null) Debug.LogError("<color=red>[CRITICAL]</color> P1 Wand Manager tidak ditemukan!");
            if (p2SerialReader == null) Debug.LogError("<color=red>[CRITICAL]</color> P2 Wand Manager tidak ditemukan!");

            Cursor.visible = false;
            uiManager.HideCursor();

            Time.timeScale = 1f;
            StartCoroutine(GameLoop());
        }

        void Update()
        {
            if (!isInputPhase) return;

            HandlePlayerInput(1);
            HandlePlayerInput(2);
        }

        private IEnumerator GameLoop()
        {
            uiManager.HideCursor();
            uiManager.ShowTutorial();
            yield return new WaitUntil(() => uiManager.IsTutorialDone);
            uiManager.HideTutorial();

            scoreManager.ResetScores();

            for (int r = 0; r < 4; r++)
            {
                int startScoreP1 = scoreManager.ScoreP1;
                int startScoreP2 = scoreManager.ScoreP2;

                currentActiveRunes = runeCounts[r];
                uiManager.UpdateDifficultyText(diffNames[r]);

                uiManager.ShowCenterText("READY?");
                uiManager.UpdateTimerUI(roundTimers[r], roundTimers[r]);
                yield return new WaitForSeconds(2f);

                runeManager.SetupRound(currentActiveRunes);
                float delay = Mathf.Max(config.minShowDelay, config.baseShowDelay - ((r + 1) * config.delayDecay));

                uiManager.ShowCenterText("MEMORIZE!");

                if (p1Animator != null) p1Animator.SetBool("IsThinking", true);
                if (p2Animator != null) p2Animator.SetBool("IsThinking", true);

                yield return StartCoroutine(runeManager.PlaySequence(delay, config.memorizationTime));

                if (p1Animator != null) p1Animator.SetBool("IsThinking", false);
                if (p2Animator != null) p2Animator.SetBool("IsThinking", false);

                uiManager.ShowCenterText("GO!");

                p1SerialReader?.FlushGesture();
                p2SerialReader?.FlushGesture();

                yield return new WaitForSeconds(0.1f);

                p1InputIdx = 0;
                p2InputIdx = 0;
                isInputPhase = true;

                float maxTime = roundTimers[r];
                float currentTime = maxTime;

                while (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                    uiManager.UpdateTimerUI(currentTime, maxTime);

                    if (p1InputIdx >= currentActiveRunes && p2InputIdx >= currentActiveRunes)
                        break;

                    yield return null;
                }

                isInputPhase = false;
                uiManager.UpdateTimerUI(0, maxTime);

                // --- Cek Pemenang Ronde untuk Animasi Win/Lose ---
                int roundScoreP1 = scoreManager.ScoreP1 - startScoreP1;
                int roundScoreP2 = scoreManager.ScoreP2 - startScoreP2;

                if (roundScoreP1 > roundScoreP2)
                {
                    if (p1Animator != null) p1Animator.SetTrigger("Win");
                    if (p2Animator != null) p2Animator.SetTrigger("Lose");
                }
                else if (roundScoreP2 > roundScoreP1)
                {
                    if (p1Animator != null) p1Animator.SetTrigger("Lose");
                    if (p2Animator != null) p2Animator.SetTrigger("Win");
                }
                else
                {
                    // Jika seri, cek apakah mereka lebih banyak menjawab benar atau salah
                    float halfRunes = currentActiveRunes / 2f;
                    if (roundScoreP1 >= halfRunes)
                    {
                        if (p1Animator != null) p1Animator.SetTrigger("Win");
                        if (p2Animator != null) p2Animator.SetTrigger("Win");
                    }
                    else
                    {
                        if (p1Animator != null) p1Animator.SetTrigger("Lose");
                        if (p2Animator != null) p2Animator.SetTrigger("Lose");
                    }
                }

                yield return new WaitForSeconds(1.5f);
                yield return StartCoroutine(runeManager.AnimateAllRunesToIdle());
                runeManager.ResetAll();
            }

            // === UPDATE: SFX TIMES UP ===
            uiManager.HideCenterText();

            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance.PlayTimesUpSound();
            }

            uiManager.ShowTimesUp();
            yield return new WaitForSeconds(3f);
            uiManager.HideTimesUp();
            // ============================

            int finalP1Score = scoreManager.ScoreP1;
            int finalP2Score = scoreManager.ScoreP2;

            PlayerPrefs.SetInt("G1_ScoreP1", finalP1Score);
            PlayerPrefs.SetInt("G1_ScoreP2", finalP2Score);
            PlayerPrefs.Save();

            // === UPDATE: SFX RESULT/PEMENANG ===
            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance.PlayResultSound();
            }

            uiManager.ShowCursor();
            uiManager.ShowResultPopup(finalP1Score, finalP2Score);
            // ===================================
        }

        private void HandlePlayerInput(int pId)
        {
            int idx = (pId == 1) ? p1InputIdx : p2InputIdx;

            if (idx >= currentActiveRunes) return;

            WandDirection dir = GetInput(pId);
            if (dir == WandDirection.None) return;

            Animator targetAnim = (pId == 1) ? p1Animator : p2Animator;
            if (targetAnim != null)
            {
                switch (dir)
                {
                    case WandDirection.Up: targetAnim.SetTrigger("Up"); break;
                    case WandDirection.Down: targetAnim.SetTrigger("Down"); break;
                    case WandDirection.Left: targetAnim.SetTrigger("Left"); break;
                    case WandDirection.Right: targetAnim.SetTrigger("Right"); break;
                }
            }

            RuneObject target = (pId == 1) ? runeManager.RunesP1[idx] : runeManager.RunesP2[idx];

            bool correct = (dir == target.AssignedDirection);
            target.ShowResult(correct);
            scoreManager.ApplyScore(pId, correct);

            if (!correct)
            {
                if (MemoryTestSoundController.Instance != null)
                {
                    MemoryTestSoundController.Instance.PlayWrongGuessSound();
                }
            }

            if (pId == 1) p1InputIdx++; else p2InputIdx++;
        }

        private WandDirection GetInput(int pId)
        {
            WandSerialReader activeReader = (pId == 1) ? p1SerialReader : p2SerialReader;

            if (activeReader != null)
            {
                string gesture = activeReader.ConsumeGesture();

                if (gesture == "U") return WandDirection.Up;
                if (gesture == "D") return WandDirection.Down;
                if (gesture == "L") return WandDirection.Left;
                if (gesture == "R") return WandDirection.Right;
            }

            if (pId == 1)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) return WandDirection.Up;
                if (Input.GetKeyDown(KeyCode.DownArrow)) return WandDirection.Down;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) return WandDirection.Left;
                if (Input.GetKeyDown(KeyCode.RightArrow)) return WandDirection.Right;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.W)) return WandDirection.Up;
                if (Input.GetKeyDown(KeyCode.S)) return WandDirection.Down;
                if (Input.GetKeyDown(KeyCode.A)) return WandDirection.Left;
                if (Input.GetKeyDown(KeyCode.D)) return WandDirection.Right;
            }

            return WandDirection.None;
        }
    }
}