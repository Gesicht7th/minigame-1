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

        private int p1InputIdx, p2InputIdx;
        private bool isInputPhase;
        private int currentActiveRunes;

        private readonly string[] diffNames = { "EASY", "MEDIUM", "HARD", "EXPERT" };
        private readonly int[] runeCounts = { 3, 4, 5, 6 };
        private readonly float[] roundTimers = { 10f, 7f, 5f, 4f };

        void Start()
        {
            Time.timeScale = 1f;
            if (SceneFlowManager.Instance == null)
            {
                var go = new GameObject("SceneFlowManager_AutoCreated");
                go.AddComponent<SceneFlowManager>();
            }

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
            scoreManager.ResetScores();

            for (int r = 0; r < 4; r++)
            {
                currentActiveRunes = runeCounts[r];
                uiManager.UpdateDifficultyText(diffNames[r]);

                uiManager.ShowCenterText("READY?");
                uiManager.UpdateTimerUI(roundTimers[r], roundTimers[r]);
                yield return new WaitForSeconds(2f);

                runeManager.SetupRound(currentActiveRunes);
                float delay = Mathf.Max(config.minShowDelay, config.baseShowDelay - ((r + 1) * config.delayDecay));

                uiManager.ShowCenterText("MEMORIZE!");
                yield return StartCoroutine(runeManager.PlaySequence(delay, config.memorizationTime));

                uiManager.ShowCenterText("GO!");
                p1InputIdx = 0; p2InputIdx = 0;
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
                yield return new WaitForSeconds(1.5f);
                runeManager.ResetAll();
            }

            uiManager.ShowCenterText("MATCH FINISHED!");
            yield return new WaitForSeconds(3f);

            // --- BAGIAN YANG DIUBAH ---
            // Kita tidak memanggil SceneFlowManager.GoTo disini lagi
            // Kita panggil fungsi Pop Up dari UIManager
            int finalP1Score = scoreManager.ScoreP1;
            int finalP2Score = scoreManager.ScoreP2;

            uiManager.ShowResultPopup(finalP1Score, finalP2Score);
        }

        private void HandlePlayerInput(int pId)
        {
            int idx = (pId == 1) ? p1InputIdx : p2InputIdx;

            if (idx >= currentActiveRunes) return;

            WandDirection dir = GetInput(pId);
            if (dir == WandDirection.None) return;

            RuneObject target = (pId == 1) ? runeManager.RunesP1[idx] : runeManager.RunesP2[idx];

            bool correct = (dir == target.AssignedDirection);
            target.ShowResult(correct);
            scoreManager.ApplyScore(pId, correct);

            if (pId == 1) p1InputIdx++; else p2InputIdx++;
        }

        private WandDirection GetInput(int pId)
        {
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