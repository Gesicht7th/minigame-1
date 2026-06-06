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
                Debug.LogWarning("[Scene] SceneFlowManager dibuat otomatis.");
            }

            StartCoroutine(GameFlow());
        }

        void Update()
        {
            bool isGameActive = (CurrentState == ReflexState.WaitGo || CurrentState == ReflexState.Draw || CurrentState == ReflexState.RoundResult);

            if (!isGameActive) return;

            if (p1Controller != null && uiManager != null)
                uiManager.UpdateActionUI(1, p1Controller.SelectedAttack);

            if (p2Controller != null && uiManager != null)
                uiManager.UpdateActionUI(2, p2Controller.SelectedAttack);
        }

        private IEnumerator GameFlow()
        {
            GlobalVirtualCursor.Instance?.Hide();
            uiManager.ShowGameScreen();

            uiManager.ShowTutorial();
            yield return new WaitUntil(() => uiManager.IsTutorialDone);
            uiManager.HideTutorial();

            scoreManager.ResetHearts();

            int round = 1;

            while (scoreManager.P1Hearts > 0 && scoreManager.P2Hearts > 0)
            {
                CurrentRound = round;
                uiManager.UpdateRoundLabel(round);

                yield return StartCoroutine(PlayRound());

                if (scoreManager.P1Hearts > 0 && scoreManager.P2Hearts > 0)
                {
                    SetState(ReflexState.Idle);
                    uiManager.ShowInterRound(round);
                    yield return new WaitForSeconds(config.interRoundDelay);
                    uiManager.HideInterRound();
                }

                round++;
            }

            yield return StartCoroutine(EndGame());
        }

        private IEnumerator PlayRound()
        {
            p1Controller.ResetForRound();
            p2Controller.ResetForRound();
            p1Visual.ResetPose();
            p2Visual.ResetPose();

            uiManager.ResetActionUI(1);
            uiManager.ResetActionUI(2);

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

            float totalTime = config.countdownStart * config.countdownStepDuration;
            uiManager.StartHoldAnimation(totalTime);

            for (int i = config.countdownStart; i >= 1; i--)
            {
                uiManager.ShowCountdown(i.ToString());

                float timer = 0f;
                while (timer < config.countdownStepDuration)
                {
                    timer += Time.deltaTime;

                    if (p1Controller.FalseStartTriggered || p2Controller.FalseStartTriggered)
                    {
                        uiManager.HideCountdown(true);
                        yield break;
                    }

                    yield return null;
                }
            }

            uiManager.HideCountdown(false);
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

        private IEnumerator EvaluateRound()
        {
            SetState(ReflexState.RoundResult);

            RpsType p1Attack = p1Controller.FiredThisRound ? p1Controller.SelectedAttack : RpsType.None;
            RpsType p2Attack = p2Controller.FiredThisRound ? p2Controller.SelectedAttack : RpsType.None;

            int winner = 0;

            if (!p1Controller.FiredThisRound && p2Controller.FiredThisRound) winner = 2;
            else if (p1Controller.FiredThisRound && !p2Controller.FiredThisRound) winner = 1;
            else if (p1Controller.FiredThisRound && p2Controller.FiredThisRound)
            {
                if (p1Attack == p2Attack) winner = 0;
                else if ((p1Attack == RpsType.Rock && p2Attack == RpsType.Scissors) ||
                         (p1Attack == RpsType.Scissors && p2Attack == RpsType.Paper) ||
                         (p1Attack == RpsType.Paper && p2Attack == RpsType.Rock))
                {
                    winner = 1;
                }
                else winner = 2;
            }

            scoreManager.RecordRoundResult(winner);

            CharacterVFXManager p1VFX = p1Visual.GetComponentInChildren<CharacterVFXManager>();
            CharacterVFXManager p2VFX = p2Visual.GetComponentInChildren<CharacterVFXManager>();

            if (p1VFX != null) p1VFX.SetMatchCondition(winner == 2, p1Attack, p2Attack);
            if (p2VFX != null) p2VFX.SetMatchCondition(winner == 1, p2Attack, p1Attack);

            AnimationDelayConfig delayConfig = config.FindDelay(p1Attack, p2Attack);
            float p1Delay = delayConfig != null ? delayConfig.p1Delay : 0f;
            float p2Delay = delayConfig != null ? delayConfig.p2Delay : 0f;

            // Trigger Shake Camera secara independen dengan nama spesifik kombinasi
            string specificProfile = $"{p1Attack}_vs_{p2Attack}";
            string fallbackProfile = (winner == 0) ? "Draw" : "Win";
            
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerProfile(specificProfile, fallbackProfile);
            }

            // --- PERBAIKAN BUG SFX KALAH ---
            if (winner == 1)
            {
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p1Visual.SetWinPose(p1Attack);
                    p1Visual.PlayFireEffect();
                    uiManager.PlayAttackSound(1, p1Attack); // P1 Menang, Putar Suara
                }, p1Delay));
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p2Visual.SetLosePose(p2Attack);
                    p2Visual.PlayFireEffect();
                    // P2 Kalah, SUARA TIDAK DIPUTAR AGAR TIDAK MENABRAK
                }, p2Delay));
            }
            else if (winner == 2)
            {
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p2Visual.SetWinPose(p2Attack);
                    p2Visual.PlayFireEffect();
                    uiManager.PlayAttackSound(2, p2Attack); // P2 Menang, Putar Suara
                }, p2Delay));
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p1Visual.SetLosePose(p1Attack);
                    p1Visual.PlayFireEffect();
                    // P1 Kalah, SUARA TIDAK DIPUTAR AGAR TIDAK MENABRAK
                }, p1Delay));
            }
            else
            {
                // Jika Draw/Seri, kedua suara tetap diputar untuk mensimulasikan benturan sihir
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p1Visual.SetDrawPose(p1Attack);
                    p1Visual.PlayFireEffect();
                    uiManager.PlayAttackSound(1, p1Attack);
                }, p1Delay));
                StartCoroutine(PlayVisualsWithDelay(() => {
                    p2Visual.SetDrawPose(p2Attack);
                    p2Visual.PlayFireEffect();
                    uiManager.PlayAttackSound(2, p2Attack);
                }, p2Delay));
            }
            // -------------------------------

            float display_t1 = p1Controller.FiredThisRound ? (p1Controller.FireTimestamp - goTimestamp) : -1f;
            float display_t2 = p2Controller.FiredThisRound ? (p2Controller.FireTimestamp - goTimestamp) : -1f;

            uiManager.ShowRoundResult(winner, display_t1, display_t2);

            float maxStartDelay = Mathf.Max(p1Delay, p2Delay);

            yield return new WaitForSeconds(2.5f + maxStartDelay);
            uiManager.HideRoundResult();
        }

        private IEnumerator PlayVisualsWithDelay(System.Action visualAction, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            visualAction?.Invoke();
        }

        private IEnumerator HandleFalseStart(bool p1False, bool p2False)
        {
            SetState(ReflexState.RoundResult);
            uiManager.HideCountdown(true);

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

        private IEnumerator EndGame()
        {
            SetState(ReflexState.GameOver);

            int gameWinner = scoreManager.GetGameWinner();
            if (gameWinner == 1) p1Visual.SetWinPose(RpsType.None);
            else if (gameWinner == 2) p2Visual.SetWinPose(RpsType.None);

            PlayerPrefs.SetInt("G3_Winner", gameWinner);
            PlayerPrefs.Save();

            uiManager.ShowTimesUp();
            yield return new WaitForSeconds(3f);
            uiManager.HideTimesUp();

            WandSerialReader[] readers = FindObjectsByType<WandSerialReader>(FindObjectsSortMode.None);
            foreach (var reader in readers)
            {
                if (reader != null) reader.ConsumeAction();
            }

            DebugInputManager.ApplyCursorMode();
            GlobalVirtualCursor.Instance?.Show();

            uiManager.ShowResultPopup(gameWinner);

            yield break;
        }

        private void SetState(ReflexState s)
        {
            CurrentState = s;
            Debug.Log($"[Reflex] → {s}");
        }
    }
}