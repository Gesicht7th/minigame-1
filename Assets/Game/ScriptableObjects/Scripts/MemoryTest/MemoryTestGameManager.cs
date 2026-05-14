// Assets/_Game/Scripts/MemoryTest/MemoryTestGameManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestGameManager : MonoBehaviour
    {
        [SerializeField] private MemoryTestConfig config;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private MemoryTestUIManager uiManager;
        [SerializeField] private MemoryTestScoreManager scoreManager;

        [SerializeField] private WandSerialReader serialReader;

        private int p1InputIdx, p2InputIdx;
        private bool isInputPhase;

        void Start()
        {

            // TAMBAHKAN INI: Auto-find jika lupa assign di Inspector
            if (serialReader == null)
                serialReader = FindObjectOfType<WandSerialReader>();

            Time.timeScale = 1f;
            // Pastikan SceneFlowManager ada (jika test langsung dari scene ini)
            if (SceneFlowManager.Instance == null)
            {
                var go = new GameObject("SceneFlowManager_AutoCreated");
                go.AddComponent<SceneFlowManager>();
                Debug.LogWarning("[Scene] SceneFlowManager dibuat otomatis. " +
                                 "Mulai dari MainMenu untuk flow yang benar.");
            }

            // Memulai alur permainan
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

            for (int r = 1; r <= config.totalRounds; r++)
            {
                uiManager.UpdateRoundText(r, config.totalRounds);
                uiManager.ShowCenterText("READY?");
                yield return new WaitForSeconds(2f);

                runeManager.SetupRound();
                float delay = Mathf.Max(config.minShowDelay, config.baseShowDelay - (r * config.delayDecay));

                uiManager.ShowCenterText("MEMORIZE!");
                yield return StartCoroutine(runeManager.PlaySequence(delay, config.memorizationTime));

                uiManager.ShowCenterText("GO!");
                p1InputIdx = 0; p2InputIdx = 0;
                isInputPhase = true;

                // Dinamis: Tunggu sampai pemain menjawab SEMUA rune yang ada di Inspector
                int maxP1 = runeManager.RunesP1.Length;
                int maxP2 = runeManager.RunesP2.Length;
                yield return new WaitUntil(() => p1InputIdx >= maxP1 && p2InputIdx >= maxP2);

                isInputPhase = false;
                yield return new WaitForSeconds(1.5f);
                runeManager.ResetAll();
            }

            uiManager.ShowCenterText("MATCH FINISHED!");
            yield return new WaitForSeconds(3f);
            SceneFlowManager.Instance.GoTo(SceneNames.ResultScene1);
        }

        private void HandlePlayerInput(int pId)
        {
            int idx = (pId == 1) ? p1InputIdx : p2InputIdx;
            int maxRunes = (pId == 1) ? runeManager.RunesP1.Length : runeManager.RunesP2.Length;

            // Cek jika sudah menjawab semua rune
            if (idx >= maxRunes) return;

            WandDirection dir = GetInput(pId);
            if (dir == WandDirection.None) return;

            RuneObject target = (pId == 1) ? runeManager.RunesP1[idx] : runeManager.RunesP2[idx];

            bool correct = (dir == target.AssignedDirection);
            target.ShowResult(correct);
            scoreManager.ApplyScore(pId, correct);

            // Maju ke rune berikutnya
            if (pId == 1) p1InputIdx++; else p2InputIdx++;
        }

        private WandDirection GetInput(int pId)
        {
            // Kita asumsikan Player 1 adalah pemegang Tongkat (Wand)
            if (pId == 1)
            {
                // 1. Baca dari ESP32
                if (serialReader != null)
                {
                    string gesture = serialReader.ConsumeGesture();
                    if (gesture == "U") return WandDirection.Up;
                    if (gesture == "D") return WandDirection.Down;
                    if (gesture == "L") return WandDirection.Left;
                    if (gesture == "R") return WandDirection.Right;
                }

                // 2. Fallback Keyboard (untuk testing tanpa hardware)
                if (Input.GetKeyDown(KeyCode.UpArrow)) return WandDirection.Up;
                if (Input.GetKeyDown(KeyCode.DownArrow)) return WandDirection.Down;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) return WandDirection.Left;
                if (Input.GetKeyDown(KeyCode.RightArrow)) return WandDirection.Right;
            }
            else // Player 2 tetap pakai Keyboard (WASD)
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