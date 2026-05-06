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
        [SerializeField] private WandAimController aimController; // P1 Aim fallback
        [SerializeField] private ShootingSystem shootingSystem; // P1 Shooter fallback
        [SerializeField] private CrystalSpawner crystalSpawner;
        [SerializeField] private HyperSmashScoreManager scoreManager;
        [SerializeField] private HyperSmashUIManager uiManager;
        [SerializeField] private CorridorBuilder corridorBuilder;

        [Header("── Scene ──────────────────────────────────")]
        [SerializeField] private string resultSceneName = "ResultScene2";

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

            // Pastikan SceneFlowManager ada (jika test langsung dari scene ini)
            if (SceneFlowManager.Instance == null)
            {
                var go = new GameObject("SceneFlowManager_AutoCreated");
                go.AddComponent<SceneFlowManager>();
                Debug.LogWarning("[Scene] SceneFlowManager dibuat otomatis. " +
                                 "Mulai dari MainMenu untuk flow yang benar.");
            }

            // Memulai alur permainan
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

            // Opsional: jika WandAimController sekarang support 2 player secara static, kamu bisa reset keduanya.
            aimController?.ResetToCenter();

            uiManager?.HideAll();
            corridorBuilder?.BuildCorridor();

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
                scoreManager.ScoreP2, // Mengirim 2 Skor
                nextRound,
                config.roundsPerGame
            );

            yield return StartCoroutine(PlayCountdown($"ROUND {nextRound}"));
            uiManager?.HideInterRoundPanel();
        }

        private IEnumerator PlayCountdown(string label)
        {
            uiManager?.ShowCountdown(label);
            yield return new WaitForSeconds(0.8f);
            for (int i = config.countdownStart; i >= 1; i--)
            {
                uiManager?.ShowCountdown(i.ToString());
                yield return new WaitForSeconds(config.countdownStepDuration);
            }
            uiManager?.ShowCountdown("GO!");
            yield return new WaitForSeconds(0.6f);
            uiManager?.HideCountdown();
        }

        private IEnumerator EndGameSequence()
        {
            SetState(HyperSmashState.GameOver);
            yield return new WaitForSeconds(0.3f);

            var newHighScores = scoreManager.TrySaveHighScores();

            // Tampilkan UI Game Over dengan 2 skor
            uiManager?.ShowGameOver(
                scoreManager.ScoreP1,
                scoreManager.ScoreP2,
                newHighScores.p1IsNew,
                newHighScores.p2IsNew
            );

            yield return new WaitForSeconds(2.5f);

            Debug.Log($"[GameManager] Loading scene: {resultSceneName}");
            SceneFlowManager.Instance.GoTo(SceneNames.ResultScene2);
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
            foreach (var c in remaining) Destroy(c.gameObject);
        }

        private void SetState(HyperSmashState s)
        {
            CurrentState = s;
            Debug.Log($"[HyperSmash] → {s}");
        }
    }
}