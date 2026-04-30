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

        // ── State ─────────────────────────────────────
        public ReflexState CurrentState { get; private set; }
        public int CurrentRound { get; private set; }

        // Timestamp saat GO dikirim
        private float goTimestamp = 0f;

        // ─────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            scoreManager.Reset();
            uiManager.HideAll();
            StartCoroutine(GameFlow());
        }

        // ── Main Flow ─────────────────────────────────
        private IEnumerator GameFlow()
        {
            uiManager.ShowGameScreen();

            for (int round = 1; round <= config.roundsPerGame; round++)
            {
                CurrentRound = round;
                uiManager.UpdateRoundLabel(round, config.roundsPerGame);

                yield return StartCoroutine(PlayRound());

                bool isLast = round == config.roundsPerGame;
                if (!isLast)
                {
                    SetState(ReflexState.Idle);
                    uiManager.ShowInterRound(scoreManager.P1RoundWins,
                                             scoreManager.P2RoundWins);
                    yield return new WaitForSeconds(config.interRoundDelay);
                    uiManager.HideInterRound();
                }
            }

            yield return StartCoroutine(EndGame());
        }

        // ── Satu Ronde ────────────────────────────────
        private IEnumerator PlayRound()
        {
            p1Controller.ResetForRound();
            p2Controller.ResetForRound();
            p1Visual.ResetPose();
            p2Visual.ResetPose();

            // ── FASE READY: kedua wand harus di bawah ──
            yield return StartCoroutine(ReadyPhase());

            // ── COUNTDOWN ──────────────────────────────
            yield return StartCoroutine(CountdownPhase());

            // Cek false start setelah countdown
            bool p1False = p1Controller.FalseStartTriggered;
            bool p2False = p2Controller.FalseStartTriggered;

            if (p1False || p2False)
            {
                yield return StartCoroutine(HandleFalseStart(p1False, p2False));
                yield break; // ronde ini dianggap selesai (atau re-do tergantung desain)
            }

            // ── RANDOM DELAY sebelum GO ─────────────
            SetState(ReflexState.WaitGo);
            float goDelay = Random.Range(config.minGoDelay, config.maxGoDelay);
            yield return new WaitForSeconds(goDelay);

            // ── DRAW! GO! ──────────────────────────────
            yield return StartCoroutine(DrawPhase());
        }

        // ── Ready Phase ───────────────────────────────
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

                // Reset timer jika ada yang tidak siap
                if (!p1Ready || !p2Ready) elapsed = 0f;

                yield return null;
            }

            uiManager.HideReadyPrompt();
        }

        // ── Countdown Phase ───────────────────────────
        private IEnumerator CountdownPhase()
        {
            SetState(ReflexState.Countdown);
            p1Controller.SetCountdownMode(true);
            p2Controller.SetCountdownMode(true);

            for (int i = config.countdownStart; i >= 1; i--)
            {
                uiManager.ShowCountdown(i.ToString());
                yield return new WaitForSeconds(config.countdownStepDuration);

                // Cek false start tiap detik
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

        // ── Draw Phase — GO! ──────────────────────────
        private IEnumerator DrawPhase()
        {
            SetState(ReflexState.Draw);

            goTimestamp = Time.time;
            p1Controller.EnableFiring();
            p2Controller.EnableFiring();

            uiManager.ShowGo();

            // Update timer display sambil menunggu kedua fire atau timeout
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

                // Jika kedua player sudah fire, selesai
                if (p1Controller.FiredThisRound && p2Controller.FiredThisRound) break;

                // Jika satu fire dan timeout berlalu cukup
                if (elapsed >= config.reactionTimeout) break;

                yield return null;
            }

            p1Controller.DisableFiring();
            p2Controller.DisableFiring();
            uiManager.HideGo();

            // Hitung pemenang ronde
            yield return StartCoroutine(EvaluateRound());
        }

        // ── Evaluate Round ────────────────────────────
        private IEnumerator EvaluateRound()
        {
            SetState(ReflexState.RoundResult);

            float t1 = p1Controller.FiredThisRound
                       ? p1Controller.FireTimestamp - goTimestamp : float.MaxValue;
            float t2 = p2Controller.FiredThisRound
                       ? p2Controller.FireTimestamp - goTimestamp : float.MaxValue;

            int winner = 0;
            if (t1 < t2) winner = 1;
            else if (t2 < t1) winner = 2;
            // Sama persis = draw (sangat jarang)

            scoreManager.RecordRoundResult(winner);

            // Visual feedback
            if (winner == 1) { p1Visual.SetWinPose(); p2Visual.SetLosePose(); }
            else if (winner == 2) { p2Visual.SetWinPose(); p1Visual.SetLosePose(); }

            p1Visual.PlayFireEffect();
            p2Visual.PlayFireEffect();

            // Tampilkan hasil ronde
            float display_t1 = p1Controller.FiredThisRound ? t1 : -1f;
            float display_t2 = p2Controller.FiredThisRound ? t2 : -1f;
            uiManager.ShowRoundResult(winner, display_t1, display_t2,
                                      scoreManager.P1RoundWins, scoreManager.P2RoundWins);

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

            // False start: yang tidak false start menang ronde
            int winner = 0;
            if (p1False && !p2False) winner = 2;
            else if (!p1False && p2False) winner = 1;
            // Keduanya false = draw

            scoreManager.RecordRoundResult(winner);
            uiManager.ShowFalseStartResult(p1False, p2False,
                                           scoreManager.P1RoundWins,
                                           scoreManager.P2RoundWins);

            yield return new WaitForSeconds(2.5f);
            uiManager.HideRoundResult();
        }

        // ── End Game ──────────────────────────────────
        private IEnumerator EndGame()
        {
            SetState(ReflexState.GameOver);

            scoreManager.FinalizeScore();
            scoreManager.TrySaveHighScores();

            int gameWinner = scoreManager.GetGameWinner();
            if (gameWinner == 1) p1Visual.SetWinPose();
            else if (gameWinner == 2) p2Visual.SetWinPose();

            // Simpan ke GameDataBridge
            GameDataBridge.Instance?.SaveReflexResult(
                scoreManager.P1TotalPoints,
                scoreManager.P2TotalPoints,
                scoreManager.P1RoundWins,
                scoreManager.P2RoundWins,
                gameWinner
            );

            uiManager.ShowGameOver(
                gameWinner,
                scoreManager.P1RoundWins,
                scoreManager.P2RoundWins
            );

            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(resultSceneName);
        }

        private void SetState(ReflexState s)
        {
            CurrentState = s;
            Debug.Log($"[Reflex] → {s}");
        }
    }
}