// Assets/_Game/Scripts/HyperSmash/HyperSmashGameManager.cs

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
        [SerializeField] private ShootingSystem shootingSystem;
        [SerializeField] private CrystalSpawner crystalSpawner;
        [SerializeField] private HyperSmashScoreManager scoreManager;
        [SerializeField] private HyperSmashUIManager uiManager;
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
            Time.timeScale = 1f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (SceneFlowManager.Instance == null)
            {
                var go = new GameObject("SceneFlowManager_AutoCreated");
                go.AddComponent<SceneFlowManager>();
                Debug.LogWarning("[Scene] SceneFlowManager dibuat otomatis.");
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
            scoreManager.ResetAll();
            aimController?.ResetToCenter();
            uiManager?.HideAll();
            
            // Mulai stage endless atau fallback ke procedural
            if (endlessStageManager != null) endlessStageManager.InitializeEndlessStage();
            else corridorBuilder?.BuildCorridor(); 

            SetState(HyperSmashState.Countdown);
            uiManager?.ShowGameScreen();
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

            foreach (var shooter in ShootingSystem.Instances)
                if (shooter != null) shooter.StartShooting();

            crystalSpawner?.StartSpawning();

            RoundTimer = config.roundDurationSeconds;
            uiManager?.ShowRoundLabel(roundNumber, config.roundsPerGame);

            yield return new WaitUntil(() => RoundTimer <= 0f);

            cameraController?.StopMoving();

            foreach (var shooter in ShootingSystem.Instances)
                if (shooter != null) shooter.StopShooting();

            crystalSpawner?.StopSpawning();
            ClearAllCrystals();

            SetState(HyperSmashState.RoundOver);
        }

        private IEnumerator ShowInterRoundScreen(int justFinishedRound)
        {
            int nextRound = justFinishedRound + 1;
            uiManager?.ShowInterRoundPanel(
                justFinishedRound,
                scoreManager.ScoreP1,
                scoreManager.ScoreP2,
                nextRound,
                config.roundsPerGame
            );

            yield return StartCoroutine(PlayCountdown($"ROUND {nextRound}"));
            uiManager?.HideInterRoundPanel();
        }

        private IEnumerator PlayCountdown(string label)
        {
            Time.timeScale = 0f; // FREEZE GAME
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
            Time.timeScale = 1f; // UNFREEZE GAME
        }

        private IEnumerator EndGameSequence()
        {
            SetState(HyperSmashState.GameOver);
            yield return new WaitForSeconds(0.3f);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // --- TAMBAHAN ENDSCREEN: Simpan Skor Game 2 ---
            PlayerPrefs.SetInt("G2_ScoreP1", scoreManager.ScoreP1);
            PlayerPrefs.SetInt("G2_ScoreP2", scoreManager.ScoreP2);
            PlayerPrefs.Save();
            // ----------------------------------------------

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
                if (c.isStaticObstacle) continue; // MENCEGAH KRISTAL DUMMIE DIHANCURKAN
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