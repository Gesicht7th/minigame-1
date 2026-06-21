// Assets/_Game/Scripts/ReflexShowdown/ReflexUIManager.cs

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk.Reflex
{
    public class ReflexUIManager : MonoBehaviour
    {
        public static ReflexUIManager Instance { get; private set; }

        [Header("── Panels ──────────────────────────────")]
        [SerializeField] private GameObject gameScreenPanel;
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private GameObject drawPanel;
        [SerializeField] private GameObject roundResultPanel;
        [SerializeField] private GameObject interRoundPanel;

        [Header("── Tutorial ──")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject tutorialPanel2;
        [SerializeField] private Button tutorialGoButton;
        [SerializeField] private Button nextSlideButton;
        [SerializeField] private Button prevSlideButton;
        public bool IsTutorialDone { get; private set; }

        [Header("── Hearts UI ───────────────────────────")]
        [SerializeField] private GameObject[] p1HeartIcons;
        [SerializeField] private GameObject[] p2HeartIcons;
        [SerializeField] private float heartPopDelay = 0.4f;

        [Header("── RPS Action UI ───────────────────────")]
        [Tooltip("0: Block(Rock), 1: Penetration(Paper), 2: Counter(Scissors)")]
        [SerializeField] private Image[] p1ActionIcons;
        [SerializeField] private Image[] p2ActionIcons;
        [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private float unselectedScale = 0.8f;
        [SerializeField] private float selectedScale = 1.1f;

        [Header("── Character Slide Animations ──────────")]
        [Tooltip("Masukkan RectTransform gambar Fura (Player 1) di sini")]
        [SerializeField] private RectTransform p1CharacterRect;
        [Tooltip("Masukkan RectTransform gambar Oura (Player 2) di sini")]
        [SerializeField] private RectTransform p2CharacterRect;
        [Tooltip("Jarak geser ke luar layar (Besarkan jika gambar kurang tersembunyi)")]
        [SerializeField] private float slideDistance = 1000f;

        [Header("── P1 Action SFX (Fura) ────────────────")]
        [SerializeField] private AudioClip p1BlockSFX;
        [SerializeField] private AudioClip p1PenetrationSFX;
        [SerializeField] private AudioClip p1CounterSFX;

        [Header("── P2 Action SFX (Oura) ────────────────")]
        [SerializeField] private AudioClip p2BlockSFX;
        [SerializeField] private AudioClip p2PenetrationSFX;
        [SerializeField] private AudioClip p2CounterSFX;

        [Header("── Game Flow SFX ───────────────────────")]
        [Tooltip("Suara saat angka 3, 2, 1 muncul")]
        [SerializeField] private AudioClip countdownTickSFX;
        [Tooltip("Suara saat tulisan GO!!! muncul")]
        [SerializeField] private AudioClip goSFX;

        [Header("── Times Up & Result SFX ───────────────")]
        [Tooltip("Suara saat waktu habis (Times Up)")]
        [SerializeField] private AudioClip timesUpSFX;
        [Tooltip("Suara saat layar Pemenang (Result) muncul")]
        [SerializeField] private AudioClip resultSFX;

        private Vector2 p1CharShownPos;
        private Vector2 p2CharShownPos;
        private Vector2 p1CharHiddenPos;
        private Vector2 p2CharHiddenPos;

        private Coroutine p1CharSlideCoroutine;
        private Coroutine p2CharSlideCoroutine;
        private Coroutine countdownScaleCoroutine;

        private bool isCountdownAudioPlayed = false;

        private RpsType lastP1Action = RpsType.None;
        private RpsType lastP2Action = RpsType.None;

        [Header("── Times Up Pop-Up ──")]
        [SerializeField] private GameObject timesUpPanel;

        [Header("── Result Pop-Up ──")]
        [SerializeField] private GameObject popupBackground;
        [SerializeField] private GameObject p1WinPanel;
        [SerializeField] private GameObject p2WinPanel;
        [SerializeField] private GameObject drawWinPanel;

        [SerializeField] private Button p1NextButton;
        [SerializeField] private Button p2NextButton;
        [SerializeField] private Button drawNextButton;

        [SerializeField] private string nextSceneName = "MainMenu";

        [Header("── HUD ─────────────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundLabelText;

        [Header("── Hold Panel ──────────────────────────")]
        [SerializeField] private GameObject holdPanel;
        [SerializeField] private Image holdImage;
        [SerializeField] private TextMeshProUGUI holdText;
        [SerializeField] private float holdBreathingSpeed = 2f;
        [SerializeField] private float holdBreathingScale = 1.15f;

        private Coroutine holdAnimCoroutine;
        private Vector3 originalHoldTextScale = Vector3.one;

        [Header("── Ready Panel ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI readyTitleText;
        [SerializeField] private Image p1ReadyIndicator;
        [SerializeField] private Image p2ReadyIndicator;
        [SerializeField] private Color readyColor = Color.green;
        [SerializeField] private Color notReadyColor = Color.white;

        [Header("── Countdown ───────────────────────────")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("── Draw Phase ──────────────────────────")]
        [SerializeField] private TextMeshProUGUI goText;
        [SerializeField] private TextMeshProUGUI p1TimerText;
        [SerializeField] private TextMeshProUGUI p2TimerText;

        [Header("── Round Result ────────────────────────")]
        [SerializeField] private TextMeshProUGUI roundWinnerText;
        [SerializeField] private TextMeshProUGUI p1TimeText;
        [SerializeField] private TextMeshProUGUI p2TimeText;
        [SerializeField] private TextMeshProUGUI falseStartText;

        [Header("── Inter Round ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI interRoundTitleText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (ReflexScoreManager.Instance != null)
                ReflexScoreManager.Instance.OnHeartsUpdated += UpdateHeartsUI;

            if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
            if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
            if (drawNextButton != null) drawNextButton.onClick.AddListener(GoToNextGame);

            if (tutorialGoButton != null)
            {
                tutorialGoButton.onClick.AddListener(TriggerTutorialDone);
            }

            if (nextSlideButton != null)
            {
                nextSlideButton.onClick.AddListener(ShowTutorialSlide2);
            }
            if (prevSlideButton != null)
            {
                prevSlideButton.onClick.AddListener(ShowTutorialSlide1);
            }

            if (p1CharacterRect != null)
            {
                p1CharShownPos = p1CharacterRect.anchoredPosition;
                p1CharHiddenPos = p1CharShownPos + new Vector2(-slideDistance, 0);
                p1CharacterRect.anchoredPosition = p1CharHiddenPos;
            }
            if (p2CharacterRect != null)
            {
                p2CharShownPos = p2CharacterRect.anchoredPosition;
                p2CharHiddenPos = p2CharShownPos + new Vector2(slideDistance, 0);
                p2CharacterRect.anchoredPosition = p2CharHiddenPos;
            }

            if (holdText != null) originalHoldTextScale = holdText.transform.localScale;
        }

        void Update()
        {
            if (!IsTutorialDone && ((tutorialPanel != null && tutorialPanel.activeSelf) || (tutorialPanel2 != null && tutorialPanel2.activeSelf)))
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    TriggerTutorialDone();
                }
            }
        }

        void OnDestroy()
        {
            if (ReflexScoreManager.Instance != null)
                ReflexScoreManager.Instance.OnHeartsUpdated -= UpdateHeartsUI;
        }

        public void TriggerTutorialDone()
        {
            IsTutorialDone = true;
        }

        private IEnumerator AnimatePopup(GameObject panel)
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);

            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                panel.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1.1f, 1.1f, 1.1f), t);
                yield return null;
            }

            elapsed = 0f;
            duration = 0.1f;
            Vector3 startScale = panel.transform.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                panel.transform.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                yield return null;
            }
            panel.transform.localScale = Vector3.one;
        }

        private IEnumerator PlayCharacterCutIn(RectTransform charRect, Vector2 shownPos, Vector2 hiddenPos, float stayDuration)
        {
            if (charRect == null) yield break;

            float duration = 0.15f;
            float elapsed = 0f;
            Vector2 startPos = charRect.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                charRect.anchoredPosition = Vector2.Lerp(startPos, shownPos, t);
                yield return null;
            }
            charRect.anchoredPosition = shownPos;

            yield return new WaitForSecondsRealtime(stayDuration);

            elapsed = 0f;
            startPos = charRect.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                charRect.anchoredPosition = Vector2.Lerp(startPos, hiddenPos, t);
                yield return null;
            }
            charRect.anchoredPosition = hiddenPos;
        }

        public void ShowTutorial()
        {
            IsTutorialDone = false;
            ShowTutorialSlide1();
        }

        public void ShowTutorialSlide1()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(true);
            if (tutorialPanel2 != null) tutorialPanel2.SetActive(false);
        }

        public void ShowTutorialSlide2()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (tutorialPanel2 != null) tutorialPanel2.SetActive(true);
        }

        public void HideTutorial()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (tutorialPanel2 != null) tutorialPanel2.SetActive(false);
        }

        public void HideAll()
        {
            gameScreenPanel?.SetActive(false);
            holdPanel?.SetActive(false);
            readyPanel?.SetActive(false);
            countdownPanel?.SetActive(false);
            drawPanel?.SetActive(false);
            roundResultPanel?.SetActive(false);
            interRoundPanel?.SetActive(false);
            tutorialPanel?.SetActive(false);
            tutorialPanel2?.SetActive(false);
            timesUpPanel?.SetActive(false);
            popupBackground?.SetActive(false);
        }

        public void ShowGameScreen()
        {
            // --- PERBAIKAN: Menyapu bersih blackscreen dari panel yang aktif tidak sengaja
            HideAll();
            // -----------------------------------------------------------------------------
            gameScreenPanel?.SetActive(true);
        }

        public void ShowHoldPanel()
        {
            if (holdPanel != null) holdPanel.SetActive(true);
            if (holdImage != null) holdImage.fillAmount = 1f;
            if (holdText != null) holdText.transform.localScale = originalHoldTextScale;
            if (holdAnimCoroutine != null) StopCoroutine(holdAnimCoroutine);
        }

        public void StartHoldAnimation(float duration)
        {
            if (holdPanel != null) holdPanel.SetActive(true);
            if (holdAnimCoroutine != null) StopCoroutine(holdAnimCoroutine);
            holdAnimCoroutine = StartCoroutine(HoldAnimationRoutine(duration));
        }

        private IEnumerator HoldAnimationRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                if (holdImage != null) holdImage.fillAmount = 1f - progress;

                if (holdText != null)
                {
                    float scale = 1f + Mathf.Abs(Mathf.Sin(elapsed * Mathf.PI * holdBreathingSpeed)) * (holdBreathingScale - 1f);
                    holdText.transform.localScale = originalHoldTextScale * scale;
                }

                yield return null;
            }

            if (holdImage != null) holdImage.fillAmount = 0f;
            if (holdText != null) holdText.transform.localScale = originalHoldTextScale;
        }

        public void ShowTimesUp()
        {
            if (timesUpPanel != null) timesUpPanel.SetActive(true);

            if (SoundManager.Instance != null && timesUpSFX != null)
            {
                SoundManager.Instance.PlaySound(timesUpSFX);
            }
        }

        public void HideTimesUp() { if (timesUpPanel != null) timesUpPanel.SetActive(false); }

        private int currentP1Hearts = -1;
        private int currentP2Hearts = -1;

        private void UpdateHeartsUI(int p1Hearts, int p2Hearts)
        {
            if (currentP1Hearts == -1)
            {
                for (int i = 0; i < p1HeartIcons.Length; i++) { if (p1HeartIcons[i] != null) { p1HeartIcons[i].SetActive(i < p1Hearts); p1HeartIcons[i].transform.localScale = Vector3.one; } }
                for (int i = 0; i < p2HeartIcons.Length; i++) { if (p2HeartIcons[i] != null) { p2HeartIcons[i].SetActive(i < p2Hearts); p2HeartIcons[i].transform.localScale = Vector3.one; } }
                currentP1Hearts = p1Hearts;
                currentP2Hearts = p2Hearts;
                return;
            }

            if (p1Hearts < currentP1Hearts)
            {
                for (int i = p1Hearts; i < currentP1Hearts; i++)
                    if (i < p1HeartIcons.Length && p1HeartIcons[i] != null) StartCoroutine(PopAndHideHeart(p1HeartIcons[i]));
            }
            else if (p1Hearts > currentP1Hearts)
            {
                for (int i = currentP1Hearts; i < p1Hearts; i++)
                    if (i < p1HeartIcons.Length && p1HeartIcons[i] != null) { p1HeartIcons[i].SetActive(true); p1HeartIcons[i].transform.localScale = Vector3.one; }
            }

            if (p2Hearts < currentP2Hearts)
            {
                for (int i = p2Hearts; i < currentP2Hearts; i++)
                    if (i < p2HeartIcons.Length && p2HeartIcons[i] != null) StartCoroutine(PopAndHideHeart(p2HeartIcons[i]));
            }
            else if (p2Hearts > currentP2Hearts)
            {
                for (int i = currentP2Hearts; i < p2Hearts; i++)
                    if (i < p2HeartIcons.Length && p2HeartIcons[i] != null) { p2HeartIcons[i].SetActive(true); p2HeartIcons[i].transform.localScale = Vector3.one; }
            }

            currentP1Hearts = p1Hearts;
            currentP2Hearts = p2Hearts;
        }

        private IEnumerator PopAndHideHeart(GameObject heartObj)
        {
            yield return new WaitForSeconds(heartPopDelay);

            if (heartObj == null) yield break;
            Transform t = heartObj.transform;
            Vector3 startScale = Vector3.one;
            Vector3 popScale = startScale * 1.5f;

            float el = 0f; float dur = 0.15f;
            while (el < dur)
            {
                el += Time.deltaTime;
                t.localScale = Vector3.Lerp(startScale, popScale, el / dur);
                yield return null;
            }

            el = 0f; dur = 0.2f;
            while (el < dur)
            {
                el += Time.deltaTime;
                t.localScale = Vector3.Lerp(popScale, Vector3.zero, el / dur);
                yield return null;
            }

            heartObj.SetActive(false);
            t.localScale = startScale;
        }

        public void ShowResultPopup(int winner)
        {
            HideAll();
            DebugInputManager.ApplyCursorMode();

            if (popupBackground != null)
            {
                StartCoroutine(AnimatePopup(popupBackground));
            }

            if (winner == 1 && p1WinPanel != null) p1WinPanel.SetActive(true);
            else if (winner == 2 && p2WinPanel != null) p2WinPanel.SetActive(true);
            else if (winner == 0 && drawWinPanel != null) drawWinPanel.SetActive(true);

            if (SoundManager.Instance != null && resultSFX != null)
            {
                SoundManager.Instance.PlaySound(resultSFX);
            }
        }

        private void GoToNextGame()
        {
            if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.GoTo(nextSceneName);
        }

        public void ShowReadyPrompt()
        {
            readyPanel?.SetActive(true);
            if (readyTitleText != null) readyTitleText.text = "HOLD WAND LOW!\nBoth players ready...";
        }
        public void UpdateReadyStatus(bool p1Ready, bool p2Ready) { if (p1ReadyIndicator != null) p1ReadyIndicator.color = p1Ready ? readyColor : notReadyColor; if (p2ReadyIndicator != null) p2ReadyIndicator.color = p2Ready ? readyColor : notReadyColor; }
        public void HideReadyPrompt() => readyPanel?.SetActive(false);

        public void ShowCountdown(string text)
        {
            countdownPanel?.SetActive(true);
            gameScreenPanel?.SetActive(false);
            if (countdownText == null) return;
            countdownText.text = text;
            countdownText.transform.localScale = Vector3.one * 2f;

            if (countdownScaleCoroutine != null) StopCoroutine(countdownScaleCoroutine);
            countdownScaleCoroutine = StartCoroutine(ScaleTo(countdownText.transform, Vector3.one, 0.3f));

            if (!isCountdownAudioPlayed)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.StopSound();
                if (SoundManager.Instance != null && countdownTickSFX != null)
                {
                    SoundManager.Instance.PlaySound(countdownTickSFX);
                }
                isCountdownAudioPlayed = true;
            }
        }

        public void HideCountdown(bool isFalseStart = false)
        {
            countdownPanel?.SetActive(false);
            holdPanel?.SetActive(false);
            if (holdAnimCoroutine != null) StopCoroutine(holdAnimCoroutine);

            if (isFalseStart)
            {
                gameScreenPanel?.SetActive(true);
                if (SoundManager.Instance != null) SoundManager.Instance.StopSound();
            }

            isCountdownAudioPlayed = false;
        }

        public void ShowGo()
        {
            drawPanel?.SetActive(true);
            if (goText != null)
            {
                goText.text = "GO!!!";
                goText.transform.localScale = Vector3.one * 3f;
                StartCoroutine(ScaleTo(goText.transform, Vector3.one, 0.2f));
            }
            if (p1TimerText != null) p1TimerText.text = "---";
            if (p2TimerText != null) p2TimerText.text = "---";

            if (SoundManager.Instance != null && goSFX != null)
            {
                SoundManager.Instance.PlaySound(goSFX);
            }
        }

        public void UpdateDrawTimers(float t1, float t2, bool p1Fired, bool p2Fired) { if (p1TimerText != null) p1TimerText.text = p1Fired ? $"{t1:F3}s" : $"{t1:F2}s..."; if (p2TimerText != null) p2TimerText.text = p2Fired ? $"{t2:F3}s" : $"{t2:F2}s..."; }

        public void HideGo()
        {
            drawPanel?.SetActive(false);
            gameScreenPanel?.SetActive(true);
        }

        public void ShowRoundResult(int winner, float t1, float t2)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(false);
            if (roundWinnerText != null)
            {
                roundWinnerText.text = winner == 0 ? "DRAW!" : $"PLAYER {winner} WINS!";
                roundWinnerText.color = winner == 1 ? new Color(0.3f, 0.6f, 1f) : winner == 2 ? new Color(1f, 0.4f, 0.2f) : Color.white;
            }
            if (p1TimeText != null) p1TimeText.text = t1 >= 0 ? $"P1: {t1:F3}s" : "P1: NO FIRE";
            if (p2TimeText != null) p2TimeText.text = t2 >= 0 ? $"P2: {t2:F3}s" : "P2: NO FIRE";
        }

        public void ShowFalseStartResult(bool p1False, bool p2False)
        {
            roundResultPanel?.SetActive(true);
            falseStartText?.gameObject.SetActive(true);
            if (falseStartText != null)
            {
                string who = (p1False && p2False) ? "BOTH PLAYERS" : p1False ? "PLAYER 1" : "PLAYER 2";
                falseStartText.text = $"FALSE START!\n{who} fired early!";
                falseStartText.color = new Color(1f, 0.8f, 0f);
            }
            if (roundWinnerText != null)
            {
                int winner = (p1False && !p2False) ? 2 : (!p1False && p2False) ? 1 : 0;
                roundWinnerText.text = winner == 0 ? "DRAW!" : $"PLAYER {winner} WINS!";
            }
        }

        public void HideRoundResult() => roundResultPanel?.SetActive(false);
        public void ShowInterRound(int round) { interRoundPanel?.SetActive(true); if (interRoundTitleText != null) interRoundTitleText.text = $"ROUND {round} COMPLETE"; }
        public void HideInterRound() => interRoundPanel?.SetActive(false);
        public void UpdateRoundLabel(int cur) { if (roundLabelText != null) roundLabelText.text = $"ROUND {cur}"; }

        private IEnumerator ScaleTo(Transform t, Vector3 target, float dur)
        {
            Vector3 start = t.localScale;
            float el = 0f;
            while (el < dur)
            {
                el += Time.deltaTime;
                t.localScale = Vector3.Lerp(start, target, el / dur);
                yield return null;
            }
            t.localScale = target;
        }

        public void ResetActionUI(int p)
        {
            if (p == 1) lastP1Action = RpsType.None;
            else lastP2Action = RpsType.None;

            if (p == 1 && p1CharacterRect != null)
            {
                if (p1CharSlideCoroutine != null) StopCoroutine(p1CharSlideCoroutine);
                p1CharacterRect.anchoredPosition = p1CharHiddenPos;
            }
            else if (p == 2 && p2CharacterRect != null)
            {
                if (p2CharSlideCoroutine != null) StopCoroutine(p2CharSlideCoroutine);
                p2CharacterRect.anchoredPosition = p2CharHiddenPos;
            }

            Image[] icons = (p == 1) ? p1ActionIcons : p2ActionIcons;
            if (icons == null) return;
            foreach (var icon in icons) if (icon != null)
                {
                    icon.color = unselectedColor;
                    icon.transform.localScale = Vector3.one * unselectedScale;
                }
        }

        public void PlayAttackSound(int player, RpsType action)
        {
            if (SoundManager.Instance == null || action == RpsType.None) return;

            if (player == 1)
            {
                if (action == RpsType.Rock && p1BlockSFX != null) SoundManager.Instance.PlaySound(p1BlockSFX);
                else if (action == RpsType.Paper && p1PenetrationSFX != null) SoundManager.Instance.PlaySound(p1PenetrationSFX);
                else if (action == RpsType.Scissors && p1CounterSFX != null) SoundManager.Instance.PlaySound(p1CounterSFX);
            }
            else if (player == 2)
            {
                if (action == RpsType.Rock && p2BlockSFX != null) SoundManager.Instance.PlaySound(p2BlockSFX);
                else if (action == RpsType.Paper && p2PenetrationSFX != null) SoundManager.Instance.PlaySound(p2PenetrationSFX);
                else if (action == RpsType.Scissors && p2CounterSFX != null) SoundManager.Instance.PlaySound(p2CounterSFX);
            }
        }

        public void UpdateActionUI(int p, RpsType s)
        {
            Image[] icons = (p == 1) ? p1ActionIcons : p2ActionIcons;
            if (icons == null || icons.Length < 3) return;

            int idx = -1;
            if (s == RpsType.Rock) idx = 0;
            else if (s == RpsType.Paper) idx = 1;
            else if (s == RpsType.Scissors) idx = 2;

            bool actionChanged = false;
            if (p == 1 && s != lastP1Action) { actionChanged = true; lastP1Action = s; }
            else if (p == 2 && s != lastP2Action) { actionChanged = true; lastP2Action = s; }

            if (actionChanged && s != RpsType.None)
            {
                if (p == 1 && p1CharacterRect != null)
                {
                    if (p1CharSlideCoroutine != null) StopCoroutine(p1CharSlideCoroutine);
                    p1CharSlideCoroutine = StartCoroutine(PlayCharacterCutIn(p1CharacterRect, p1CharShownPos, p1CharHiddenPos, 1.0f));
                }
                else if (p == 2 && p2CharacterRect != null)
                {
                    if (p2CharSlideCoroutine != null) StopCoroutine(p2CharSlideCoroutine);
                    p2CharSlideCoroutine = StartCoroutine(PlayCharacterCutIn(p2CharacterRect, p2CharShownPos, p2CharHiddenPos, 1.0f));
                }
            }

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                {
                    bool isSel = (i == idx);
                    icons[i].color = isSel ? selectedColor : unselectedColor;
                    icons[i].transform.localScale = Vector3.one * (isSel ? selectedScale : unselectedScale);
                }
            }
        }
    }
}