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

        [Header("── Wand Inputs ──────────────────────")]
        [SerializeField] private WandSerialReader p1SerialReader;
        [SerializeField] private WandSerialReader p2SerialReader;

        private int p1InputIdx, p2InputIdx;
        private bool isInputPhase;

        private int currentActiveRunes;

        private readonly string[] diffNames =
        {
            "EASY",
            "MEDIUM",
            "HARD",
            "EXPERT"
        };

        private readonly int[] runeCounts =
        {
            3,
            4,
            5,
            6
        };

        private readonly float[] roundTimers =
        {
            10f,
            7f,
            5f,
            4f
        };

        // ============================================================
        // START
        // ============================================================

        void Start()
        {
            // ========================================================
            // FIND SERIAL READERS
            // ========================================================

            WandSerialReader[] readers =
                FindObjectsOfType<WandSerialReader>();

            foreach (var reader in readers)
            {
                if (reader.name.ToUpper().Contains("P1") ||
                    reader.name.ToUpper().Contains("PLAYER1"))
                {
                    if (p1SerialReader == null)
                    {
                        p1SerialReader = reader;
                    }
                }
                else if (
                    reader.name.ToUpper().Contains("P2") ||
                    reader.name.ToUpper().Contains("PLAYER2"))
                {
                    if (p2SerialReader == null)
                    {
                        p2SerialReader = reader;
                    }
                }
            }

            if (p1SerialReader == null && readers.Length > 0)
            {
                p1SerialReader = readers[0];
            }

            if (p2SerialReader == null && readers.Length > 1)
            {
                p2SerialReader =
                    (readers[0] == p1SerialReader)
                    ? readers[1]
                    : readers[0];
            }

            // ========================================================
            // DEBUG
            // ========================================================

            if (p1SerialReader == null)
            {
                Debug.LogError(
                    "<color=red>[CRITICAL]</color> P1 Wand Manager tidak ditemukan!");
            }

            if (p2SerialReader == null)
            {
                Debug.LogError(
                    "<color=red>[CRITICAL]</color> P2 Wand Manager tidak ditemukan!");
            }

            // ========================================================
            // CURSOR SYSTEM
            // ========================================================

            // Hide REAL cursor sepanjang game
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState =
                CursorLockMode.Locked;

            // Hide VIRTUAL pointer di awal game
            CursorGlobal.HideVirtualCursor();

            // ========================================================
            // START GAME
            // ========================================================

            Time.timeScale = 1f;

            StartCoroutine(GameLoop());
        }

        // ============================================================
        // UPDATE
        // ============================================================

        void Update()
        {
            // ========================================================
            // ESC = SHOW REAL CURSOR
            // ========================================================

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState =
                    CursorLockMode.None;
            }

            // ========================================================
            // GAME INPUT
            // ========================================================

            if (!isInputPhase) return;

            HandlePlayerInput(1);
            HandlePlayerInput(2);
        }

        // ============================================================
        // GAME LOOP
        // ============================================================

        private IEnumerator GameLoop()
        {
            // ========================================================
            // TUTORIAL
            // ========================================================

            uiManager.ShowTutorial();

            yield return new WaitUntil(
                () => uiManager.IsTutorialDone);

            uiManager.HideTutorial();

            // ========================================================
            // RESET SCORE
            // ========================================================

            scoreManager.ResetScores();

            // ========================================================
            // ROUNDS
            // ========================================================

            for (int r = 0; r < 4; r++)
            {
                currentActiveRunes = runeCounts[r];

                uiManager.UpdateDifficultyText(
                    diffNames[r]);

                // ====================================================
                // READY
                // ====================================================

                uiManager.ShowCenterText("READY?");

                uiManager.UpdateTimerUI(
                    roundTimers[r],
                    roundTimers[r]);

                yield return new WaitForSeconds(2f);

                // ====================================================
                // SETUP ROUND
                // ====================================================

                runeManager.SetupRound(currentActiveRunes);

                float delay =
                    Mathf.Max(
                        config.minShowDelay,
                        config.baseShowDelay -
                        ((r + 1) * config.delayDecay));

                // ====================================================
                // MEMORIZE
                // ====================================================

                uiManager.ShowCenterText("MEMORIZE!");

                yield return StartCoroutine(
                    runeManager.PlaySequence(
                        delay,
                        config.memorizationTime));

                // ====================================================
                // GO
                // ====================================================

                uiManager.ShowCenterText("GO!");

                p1SerialReader?.FlushGesture();
                p2SerialReader?.FlushGesture();

                yield return new WaitForSeconds(0.1f);

                p1InputIdx = 0;
                p2InputIdx = 0;

                isInputPhase = true;

                // ====================================================
                // ROUND TIMER
                // ====================================================

                float maxTime = roundTimers[r];
                float currentTime = maxTime;

                while (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;

                    uiManager.UpdateTimerUI(
                        currentTime,
                        maxTime);

                    if (p1InputIdx >= currentActiveRunes &&
                        p2InputIdx >= currentActiveRunes)
                    {
                        break;
                    }

                    yield return null;
                }

                // ====================================================
                // ROUND END
                // ====================================================

                isInputPhase = false;

                uiManager.UpdateTimerUI(0, maxTime);

                yield return new WaitForSeconds(1.5f);

                yield return StartCoroutine(
                    runeManager.AnimateAllRunesToIdle());

                runeManager.ResetAll();
            }

            // ========================================================
            // TIMES UP
            // ========================================================

            uiManager.HideCenterText();

            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance
                    .PlayTimesUpSound();
            }

            uiManager.ShowTimesUp();

            yield return new WaitForSeconds(3f);

            uiManager.HideTimesUp();

            // ========================================================
            // FINAL SCORE
            // ========================================================

            int finalP1Score =
                scoreManager.ScoreP1;

            int finalP2Score =
                scoreManager.ScoreP2;

            PlayerPrefs.SetInt(
                "G1_ScoreP1",
                finalP1Score);

            PlayerPrefs.SetInt(
                "G1_ScoreP2",
                finalP2Score);

            PlayerPrefs.Save();

            // ========================================================
            // RESULT SFX
            // ========================================================

            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance
                    .PlayResultSound();
            }

            // ========================================================
            // SHOW VIRTUAL POINTER
            // ========================================================

            CursorGlobal.ShowVirtualCursor();

            // ========================================================
            // RESULT POPUP
            // ========================================================

            uiManager.ShowResultPopup(
                finalP1Score,
                finalP2Score);
        }

        // ============================================================
        // PLAYER INPUT
        // ============================================================

        private void HandlePlayerInput(int pId)
        {
            int idx =
                (pId == 1)
                ? p1InputIdx
                : p2InputIdx;

            if (idx >= currentActiveRunes)
            {
                return;
            }

            WandDirection dir =
                GetInput(pId);

            if (dir == WandDirection.None)
            {
                return;
            }

            RuneObject target =
                (pId == 1)
                ? runeManager.RunesP1[idx]
                : runeManager.RunesP2[idx];

            bool correct =
                (dir == target.AssignedDirection);

            target.ShowResult(correct);

            scoreManager.ApplyScore(
                pId,
                correct);

            if (!correct)
            {
                if (MemoryTestSoundController.Instance != null)
                {
                    MemoryTestSoundController.Instance
                        .PlayWrongGuessSound();
                }
            }

            if (pId == 1)
            {
                p1InputIdx++;
            }
            else
            {
                p2InputIdx++;
            }
        }

        // ============================================================
        // GET INPUT
        // ============================================================

        private WandDirection GetInput(int pId)
        {
            WandSerialReader activeReader =
                (pId == 1)
                ? p1SerialReader
                : p2SerialReader;

            // ========================================================
            // WAND INPUT
            // ========================================================

            if (activeReader != null)
            {
                string gesture =
                    activeReader.ConsumeGesture();

                if (gesture == "U")
                    return WandDirection.Up;

                if (gesture == "D")
                    return WandDirection.Down;

                if (gesture == "L")
                    return WandDirection.Left;

                if (gesture == "R")
                    return WandDirection.Right;
            }

            // ========================================================
            // KEYBOARD FALLBACK
            // ========================================================

            if (pId == 1)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    return WandDirection.Up;

                if (Input.GetKeyDown(KeyCode.DownArrow))
                    return WandDirection.Down;

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    return WandDirection.Left;

                if (Input.GetKeyDown(KeyCode.RightArrow))
                    return WandDirection.Right;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.W))
                    return WandDirection.Up;

                if (Input.GetKeyDown(KeyCode.S))
                    return WandDirection.Down;

                if (Input.GetKeyDown(KeyCode.A))
                    return WandDirection.Left;

                if (Input.GetKeyDown(KeyCode.D))
                    return WandDirection.Right;
            }

            return WandDirection.None;
        }
    }
}