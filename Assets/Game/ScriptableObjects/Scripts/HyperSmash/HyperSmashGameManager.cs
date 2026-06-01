using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardPunk.HyperSmash
{
    public class HyperSmashGameManager : MonoBehaviour
    {
        public static HyperSmashGameManager Instance { get; private set; }

        [Header("── References ────────────────────────────")]
        [SerializeField] private HyperSmashConfig config;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private WandAimController aimController;

        [SerializeField] private CrystalSpawner crystalSpawner;
        [SerializeField] private HyperSmashScoreManager scoreManager;
        [SerializeField] private HyperSmashDummieUIManager uiManager;
        [SerializeField] private CorridorBuilder corridorBuilder;
        [SerializeField] private EndlessStageManager endlessStageManager;

        public HyperSmashState CurrentState { get; private set; }
        public float RoundTimer { get; private set; }
        public int CurrentRound { get; private set; }
        public bool IsPlaying => CurrentState == HyperSmashState.Playing;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            GlobalVirtualCursor.Instance?.Hide();
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (SceneFlowManager.Instance == null)
            {
                var go = new GameObject("SceneFlowManager_AutoCreated");
                go.AddComponent<SceneFlowManager>();
            }

            StartCoroutine(GameFlow());
        }

        void Update()
        {
            if (CurrentState != HyperSmashState.Playing) return;

            RoundTimer -= Time.deltaTime;
            uiManager?.UpdateTimer(RoundTimer, config.roundDurationSeconds);

            if (RoundTimer <= 0f) RoundTimer = 0f;
        }

        private IEnumerator GameFlow()
        {
            GlobalVirtualCursor.Instance?.Hide();
            scoreManager.ResetAll();
            aimController?.ResetToCenter();
            uiManager?.HideAll();

            if (endlessStageManager != null) endlessStageManager.InitializeEndlessStage();
            else corridorBuilder?.BuildCorridor();

            uiManager?.ShowTutorial();
            yield return new WaitUntil(() => uiManager != null && uiManager.IsTutorialDone);
            uiManager?.HideTutorial();

            uiManager?.ShowGameScreen();

            SetState(HyperSmashState.Countdown);
            yield return StartCoroutine(PlayCountdown("GET READY!"));

            for (int round = 1; round <= config.roundsPerGame; round++)
            {
                CurrentRound = round;
                yield return StartCoroutine(PlayRound(round));

                if (round == config.roundsPerGame)
                    yield return StartCoroutine(EndGameSequence());
                else
                    yield return StartCoroutine(ShowInterRoundScreen(round));
            }
        }

        private IEnumerator PlayRound(int roundNumber)
        {
            SetState(HyperSmashState.Playing);
            cameraController?.ResetPosition(Vector3.zero);
            cameraController?.StartMoving();

            // Panggil sistem Auto Fire baru jika ada di scene
            DualCrosshairController.Instance?.StartAutoFire();

            crystalSpawner?.StartSpawning();
            RoundTimer = config.roundDurationSeconds;
            uiManager?.ShowRoundLabel(roundNumber, config.roundsPerGame);

            yield return new WaitUntil(() => RoundTimer <= 0f);

            cameraController?.StopMoving();

            // Hentikan sistem Auto Fire baru
            DualCrosshairController.Instance?.StopAutoFire();

            crystalSpawner?.StopSpawning();

            // --- AUDIO & POP-UP TIMES UP ---
            if (uiManager != null)
            {
                HyperSmashSoundController.Instance?.PlayTimesUpSound();
                yield return StartCoroutine(uiManager.ShowTimesUp());
            }
            // -------------------------------

            ClearAllCrystals();
            SetState(HyperSmashState.RoundOver);
        }

        private IEnumerator ShowInterRoundScreen(int justFinishedRound)
        {
            int nextRound = justFinishedRound + 1;
            uiManager?.ShowInterRoundPanel(
                justFinishedRound, scoreManager.ScoreP1, scoreManager.ScoreP2, nextRound, config.roundsPerGame
            );

            yield return StartCoroutine(PlayCountdown($"ROUND {nextRound}"));
            uiManager?.HideInterRoundPanel();
        }

        private IEnumerator PlayCountdown(string label)
        {
            Time.timeScale = 0f;
            uiManager?.ShowCountdown(label);
            yield return new WaitForSecondsRealtime(0.8f);
            for (int i = config.countdownStart; i >= 1; i--)
            {
                uiManager?.ShowCountdown(i.ToString());
                yield return new WaitForSecondsRealtime(config.countdownStepDuration);
            }
            uiManager?.ShowCountdown("GO!");
            yield return new WaitForSecondsRealtime(0.6f);
            uiManager?.HideCountdown();
            Time.timeScale = 1f;
        }

        private IEnumerator EndGameSequence()
        {
            SetState(HyperSmashState.GameOver);
            yield return new WaitForSeconds(0.3f);

            // Flush buffered inputs before showing cursor
            WandSerialReader[] readers = FindObjectsByType<WandSerialReader>(FindObjectsSortMode.None);
            foreach (var reader in readers)
            {
                if (reader != null) reader.ConsumeAction();
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GlobalVirtualCursor.Instance?.Show();

            PlayerPrefs.SetInt("G2_ScoreP1", scoreManager.ScoreP1);
            PlayerPrefs.SetInt("G2_ScoreP2", scoreManager.ScoreP2);
            PlayerPrefs.Save();

            // --- AUDIO RESULT SCENE ---
            HyperSmashSoundController.Instance?.PlayResultSound();
            // --------------------------

            uiManager?.ShowResultPopup(scoreManager.ScoreP1, scoreManager.ScoreP2);
        }

        public void OnCrystalKilled(Crystal crystal, PlayerIndex killer)
        {
            if (CurrentState != HyperSmashState.Playing) return;
            scoreManager.RegisterCrystalKill(killer, crystal);
            uiManager?.ShowScorePopup(crystal.ScoreValue, crystal.transform.position);
        }

        public void RegisterShot(PlayerIndex player) => scoreManager.RegisterShot(player);
        public void RegisterHit(PlayerIndex player) => scoreManager.RegisterHit(player);

        private void ClearAllCrystals()
        {
            Crystal[] remaining = FindObjectsByType<Crystal>(FindObjectsSortMode.None);
            foreach (var c in remaining)
            {
                if (c.isStaticObstacle) continue;
                Destroy(c.gameObject);
            }
        }

        private void SetState(HyperSmashState s)
        {
            CurrentState = s;
            Debug.Log($"[HyperSmash] → {s}");
        }
    }
}