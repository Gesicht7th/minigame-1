// Assets/_Game/Scripts/ReflexShowdown/ReflexGameManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardPunk.Reflex
{
    public enum ReflexState
    {
        Idle, Ready, Countdown, WaitGo, Draw, RoundResult, GameOver
    }

    public class ReflexGameManager : MonoBehaviour
    {
        public static ReflexGameManager Instance { get; private set; }

        [Header("── References ─────────────────────────")]
        [SerializeField] private ReflexConfig config;
        [SerializeField] private PlayerReflexController p1Controller;
        [SerializeField] private PlayerReflexController p2Controller;
        [SerializeField] private ReflexScoreManager scoreManager;
        [SerializeField] private ReflexUIManager uiManager;
        [SerializeField] private CharacterVisual p1Visual;
        [SerializeField] private CharacterVisual p2Visual;

        [Header("── Scene ───────────────────────────────")]
        [SerializeField] private string resultSceneName = "ResultScene3";

        public ReflexState CurrentState { get; private set; }
        public int CurrentRound { get; private set; }

        private float goTimestamp = 0f;

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
                Debug.LogWarning("[Scene] SceneFlowManager dibuat otomatis. Mulai dari MainMenu untuk flow yang benar.");
            }

            StartCoroutine(GameFlow());
        }

        // ── Main Flow (Sistem Hearts) ─────────────────
        private IEnumerator GameFlow()
        {
            uiManager.ShowGameScreen();
            scoreManager.ResetHearts(); // Reset nyawa jadi 3-3

            int round = 1;

            // Loop terus berjalan selama KEDUA pemain masih memiliki nyawa
            while (scoreManager.P1Hearts > 0 && scoreManager.P2Hearts > 0)
            {
                CurrentRound = round;
                uiManager.UpdateRoundLabel(round);

                yield return StartCoroutine(PlayRound());

                // Jika setelah main ronde ini keduanya masih hidup, tunjukkan Inter-Round
                if (scoreManager.P1Hearts > 0 && scoreManager.P2Hearts > 0)
                {
                    SetState(ReflexState.Idle);
                    uiManager.ShowInterRound(round);
                    yield return new WaitForSeconds(config.interRoundDelay);
                    uiManager.HideInterRound();
                }

                round++;
            }

            // Jika keluar dari while loop, berarti nyawa salah satu pemain habis
            yield return StartCoroutine(EndGame());
        }

        private IEnumerator PlayRound()
        {
            p1Controller.ResetForRound();
            p2Controller.ResetForRound();
            p1Visual.ResetPose();
            p2Visual.ResetPose();

            yield return StartCoroutine(ReadyPhase());
            yield return StartCoroutine(CountdownPhase());

            bool p1False = p1Controller.FalseStartTriggered;
            bool p2False = p2Controller.FalseStartTriggered;

            if (p1False || p2False)
            {
                yield return StartCoroutine(HandleFalseStart(p1False, p2False));
                yield break;
            }

            SetState(ReflexState.WaitGo);
            float goDelay = Random.Range(config.minGoDelay, config.maxGoDelay);
            yield return new WaitForSeconds(goDelay);

            yield return StartCoroutine(DrawPhase());
        }

        private IEnumerator ReadyPhase()
        {
            SetState(ReflexState.Ready);
            uiManager.ShowReadyPrompt();

            float elapsed = 0f;
            while (elapsed < config.readyPhaseDuration)
            {
                elapsed += Time.deltaTime;
                bool p1Ready = p1Controller.IsWandLow;
                bool p2Ready = p2Controller.IsWandLow;
                uiManager.UpdateReadyStatus(p1Ready, p2Ready);

                if (!p1Ready || !p2Ready) elapsed = 0f;
                yield return null;
            }
            uiManager.HideReadyPrompt();
        }

        private IEnumerator CountdownPhase()
        {
            SetState(ReflexState.Countdown);
            p1Controller.SetCountdownMode(true);
            p2Controller.SetCountdownMode(true);

            for (int i = config.countdownStart; i >= 1; i--)
            {
                uiManager.ShowCountdown(i.ToString());
                yield return new WaitForSeconds(config.countdownStepDuration);

                if (p1Controller.FalseStartTriggered || p2Controller.FalseStartTriggered)
                {
                    uiManager.HideCountdown();
                    yield break;
                }
            }

            uiManager.HideCountdown();
            p1Controller.SetCountdownMode(false);
            p2Controller.SetCountdownMode(false);
        }

        private IEnumerator DrawPhase()
        {
            SetState(ReflexState.Draw);
            goTimestamp = Time.time;
            p1Controller.EnableFiring();
            p2Controller.EnableFiring();
            uiManager.ShowGo();

            float elapsed = 0f;
            while (elapsed < config.reactionTimeout)
            {
                elapsed = Time.time - goTimestamp;
                uiManager.UpdateDrawTimers(
                    p1Controller.FiredThisRound ? (p1Controller.FireTimestamp - goTimestamp) : elapsed,
                    p2Controller.FiredThisRound ? (p2Controller.FireTimestamp - goTimestamp) : elapsed,
                    p1Controller.FiredThisRound,
                    p2Controller.FiredThisRound
                );

                if (p1Controller.FiredThisRound && p2Controller.FiredThisRound) break;
                if (elapsed >= config.reactionTimeout) break;
                yield return null;
            }

            p1Controller.DisableFiring();
            p2Controller.DisableFiring();
            uiManager.HideGo();

            yield return StartCoroutine(EvaluateRound());
        }

        // ── Evaluate Round (Sistem Batu Gunting Kertas) ──
        private IEnumerator EvaluateRound()
        {
            SetState(ReflexState.RoundResult);

            RpsType p1Attack = p1Controller.SelectedAttack;
            RpsType p2Attack = p2Controller.SelectedAttack;

            int winner = 0; // 0 = draw, 1 = p1, 2 = p2

            // Evaluasi Timeout (jika ada yang diam saja)
            if (!p1Controller.FiredThisRound && p2Controller.FiredThisRound) winner = 2;
            else if (p1Controller.FiredThisRound && !p2Controller.FiredThisRound) winner = 1;
            else if (p1Controller.FiredThisRound && p2Controller.FiredThisRound)
            {
                // Evaluasi Batu Gunting Kertas
                if (p1Attack == p2Attack)
                {
                    winner = 0; // Seri / Draw
                    Debug.Log($"Seri! Keduanya mengeluarkan {p1Attack}");
                }
                else if ((p1Attack == RpsType.Rock && p2Attack == RpsType.Scissors) ||
                         (p1Attack == RpsType.Scissors && p2Attack == RpsType.Paper) ||
                         (p1Attack == RpsType.Paper && p2Attack == RpsType.Rock))
                {
                    winner = 1;
                }
                else
                {
                    winner = 2;
                }
            }

            scoreManager.RecordRoundResult(winner);

            // Visual feedback
            if (winner == 1) { p1Visual.SetWinPose(); p2Visual.SetLosePose(); }
            else if (winner == 2) { p2Visual.SetWinPose(); p1Visual.SetLosePose(); }

            p1Visual.PlayFireEffect();
            p2Visual.PlayFireEffect();

            // Waktu (reaction time) dikirim untuk UI
            float display_t1 = p1Controller.FiredThisRound ? (p1Controller.FireTimestamp - goTimestamp) : -1f;
            float display_t2 = p2Controller.FiredThisRound ? (p2Controller.FireTimestamp - goTimestamp) : -1f;

            uiManager.ShowRoundResult(winner, display_t1, display_t2);

            yield return new WaitForSeconds(2.5f);
            uiManager.HideRoundResult();
        }

        // ── False Start ───────────────────────────────
        private IEnumerator HandleFalseStart(bool p1False, bool p2False)
        {
            SetState(ReflexState.RoundResult);
            uiManager.HideCountdown();

            if (p1False) p1Visual.PlayFalseStartEffect();
            if (p2False) p2Visual.PlayFalseStartEffect();

            int winner = 0;
            if (p1False && !p2False) winner = 2;
            else if (!p1False && p2False) winner = 1;

            scoreManager.RecordRoundResult(winner);

            uiManager.ShowFalseStartResult(p1False, p2False);

            yield return new WaitForSeconds(2.5f);
            uiManager.HideRoundResult();
        }

        // ── End Game (Sistem Pop-Up) ──────────────────
        private IEnumerator EndGame()
        {
            SetState(ReflexState.GameOver);

            int gameWinner = scoreManager.GetGameWinner();
            if (gameWinner == 1) p1Visual.SetWinPose();
            else if (gameWinner == 2) p2Visual.SetWinPose();

            // Panggil Pop-Up Result dari UIManager
            uiManager.ShowResultPopup(gameWinner);

            yield break; // Selesai, menunggu tombol NEXT ditekan
        }

        private void SetState(ReflexState s)
        {
            CurrentState = s;
            Debug.Log($"[Reflex] → {s}");
        }
    }
}