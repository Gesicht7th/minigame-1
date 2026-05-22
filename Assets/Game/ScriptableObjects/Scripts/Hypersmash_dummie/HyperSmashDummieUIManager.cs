using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WizardPunk;
using WizardPunk.HyperSmash;

public class HyperSmashDummieUIManager : MonoBehaviour
{
    public static HyperSmashDummieUIManager Instance { get; private set; }

    [Header("── Panels ─────────────────────────────────")]
    [SerializeField] private GameObject gameScreenPanel;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameOverPanel2;
    [SerializeField] private GameObject interRoundPanel;

    [Header("── HUD Universal ───────────────────────────")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBarFill;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI roundLabelText;

    [Header("── HUD Player 1 ────────────────────────────")]
    [SerializeField] private TextMeshProUGUI scoreTextP1;
    [SerializeField] private TextMeshProUGUI accuracyTextP1;
    [SerializeField] private Image crosshairImageP1;

    [Header("── HUD Player 2 ────────────────────────────")]
    [SerializeField] private TextMeshProUGUI scoreTextP2;
    [SerializeField] private TextMeshProUGUI accuracyTextP2;
    [SerializeField] private Image crosshairImageP2;

    [Header("── Crosshair Config ────────────────────────")]
    [SerializeField] private Color crosshairDefaultColor = Color.white;
    [SerializeField] private Color crosshairHitColor = new Color(1f, 0.9f, 0f);

    [Header("── Countdown & Inter-Round ─────────────────")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI interRoundTitleText;
    [SerializeField] private TextMeshProUGUI interRoundNextText;
    [SerializeField] private TextMeshProUGUI interRoundScoreTextP1;
    [SerializeField] private TextMeshProUGUI interRoundScoreTextP2;

    [Header("── Game Over ───────────────────────────────")]
    [SerializeField] private TextMeshProUGUI gameOverScoreTextP1;
    [SerializeField] private TextMeshProUGUI gameOverScoreTextP2;
    [SerializeField] private GameObject newHighScoreBadgeP1;
    [SerializeField] private GameObject newHighScoreBadgeP2;

    [Header("── Result Pop-Up ──")]
    [SerializeField] private GameObject popupBackground;
    [SerializeField] private GameObject p1WinPanel;
    [SerializeField] private GameObject p2WinPanel;
    [SerializeField] private GameObject drawWinPanel;

    [SerializeField] private TextMeshProUGUI p1PtsText;
    [SerializeField] private TextMeshProUGUI p2PtsText;
    [SerializeField] private TextMeshProUGUI drawPtsText;

    [SerializeField] private Button p1NextButton;
    [SerializeField] private Button p2NextButton;
    [SerializeField] private Button drawNextButton;

    [SerializeField] private string nextSceneName = "MainMenu";

    private Coroutine flashCoroutineP1;
    private Coroutine flashCoroutineP2;
    private string lastGameState = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Menyembunyikan semua panel yang tidak perlu dan hanya menampilkan HUD Game
        HideAll();
        if (gameScreenPanel != null) gameScreenPanel.SetActive(true);

        if (HyperSmashScoreManager.Instance != null)
            HyperSmashScoreManager.Instance.OnScoreChanged += OnScoreChanged;

        if (p1NextButton != null) p1NextButton.onClick.AddListener(GoToNextGame);
        if (p2NextButton != null) p2NextButton.onClick.AddListener(GoToNextGame);
        if (drawNextButton != null) drawNextButton.onClick.AddListener(GoToNextGame);
    }

    void Update()
    {
        // Pergerakan crosshair dihapus dari UIManager ini
        // Posisi dan Input Crosshair ditangani secara penuh oleh DualCrosshairController
        
        UpdateSpeedDisplay();

        if (HyperSmashScoreManager.Instance != null)
        {
            UpdateAccuracy(PlayerIndex.Player1, HyperSmashScoreManager.Instance.GetAccuracy(PlayerIndex.Player1));
            UpdateAccuracy(PlayerIndex.Player2, HyperSmashScoreManager.Instance.GetAccuracy(PlayerIndex.Player2));
        }

        // Membaca timer dan ronde dari GameManager yang berjalan di latar belakang
        if (HyperSmashGameManager.Instance != null)
        {
            float currentTimer = HyperSmashGameManager.Instance.RoundTimer;
            UpdateTimer(currentTimer, 60f); // Asumsi durasi standar 60 detik untuk progress bar
            
            // Juga update Round Label agar tidak nyangkut
            ShowRoundLabel(HyperSmashGameManager.Instance.CurrentRound, 3); // Asumsi total 3 ronde

            // Deteksi perubahan state (Countdown, InterRound, GameOver)
            string currentState = HyperSmashGameManager.Instance.CurrentState.ToString();
            if (currentState != lastGameState)
            {
                lastGameState = currentState;
                if (currentState == "Countdown")
                {
                    StartCoroutine(PlayDummyCountdown("GET READY!"));
                }
                else if (currentState == "RoundOver")
                {
                    int nextRound = HyperSmashGameManager.Instance.CurrentRound + 1;
                    StartCoroutine(PlayDummyCountdown($"ROUND {nextRound}"));
                }
                else if (currentState == "GameOver")
                {
                    int p1 = HyperSmashScoreManager.Instance != null ? HyperSmashScoreManager.Instance.ScoreP1 : 0;
                    int p2 = HyperSmashScoreManager.Instance != null ? HyperSmashScoreManager.Instance.ScoreP2 : 0;
                    ShowResultPopup(p1, p2);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (HyperSmashScoreManager.Instance != null)
            HyperSmashScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    #region Panels
    public void HideAll()
    {
        if (gameScreenPanel != null) gameScreenPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverPanel2 != null) gameOverPanel2.SetActive(false);
        if (interRoundPanel != null) interRoundPanel.SetActive(false);
        if (popupBackground != null) popupBackground.SetActive(false);
    }

    public void ShowGameScreen()
    {
        HideAll();
        if (gameScreenPanel != null) gameScreenPanel.SetActive(true);
        UpdateScore(PlayerIndex.Player1, 0);
        UpdateScore(PlayerIndex.Player2, 0);
    }

    public void ShowResultPopup(int scoreP1, int scoreP2)
    {
        HideAll();

        if (popupBackground != null) popupBackground.SetActive(true);

        if (scoreP1 > scoreP2)
        {
            if (p1WinPanel != null) p1WinPanel.SetActive(true);
            if (p1PtsText != null) p1PtsText.text = scoreP1.ToString();
        }
        else if (scoreP2 > scoreP1)
        {
            if (p2WinPanel != null) p2WinPanel.SetActive(true);
            if (p2PtsText != null) p2PtsText.text = scoreP2.ToString();
        }
        else
        {
            if (drawWinPanel != null) drawWinPanel.SetActive(true);
            if (drawPtsText != null) drawPtsText.text = scoreP1.ToString();
        }
    }

    private void GoToNextGame()
    {
        Debug.Log("Melanjutkan ke game seterusnya: " + nextSceneName);
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.GoTo(nextSceneName);
    }
    #endregion

    #region Inter Round & Countdown
    public void ShowRoundLabel(int current, int total)
    {
        if (roundLabelText != null) roundLabelText.text = $"ROUND {current} / {total}";
    }

    public void ShowInterRoundPanel(int finishedRound, int scoreP1, int scoreP2, int nextRound, int totalRounds)
    {
        if (interRoundPanel != null) interRoundPanel.SetActive(true);
        if (interRoundTitleText != null) interRoundTitleText.text = $"ROUND {finishedRound} COMPLETE";
        if (interRoundScoreTextP1 != null) interRoundScoreTextP1.text = $"P1 Score: {scoreP1}";
        if (interRoundScoreTextP2 != null) interRoundScoreTextP2.text = $"P2 Score: {scoreP2}";
        if (interRoundNextText != null) interRoundNextText.text = $"Next: Round {nextRound} / {totalRounds}";
    }

    public void HideInterRoundPanel()
    {
        if (interRoundPanel != null) interRoundPanel.SetActive(false);
    }

    public void ShowCountdown(string text)
    {
        if (countdownPanel != null) countdownPanel.SetActive(true);
        if (countdownText == null) return;
        countdownText.text = text;
        countdownText.transform.localScale = Vector3.one * 2f;
        StartCoroutine(ScaleTo(countdownText.transform, Vector3.one, 0.35f));
    }

    public void HideCountdown()
    {
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    private IEnumerator ScaleTo(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Gunakan unscaled agar jalan saat Time.timeScale = 0
            t.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        t.localScale = target;
    }

    private IEnumerator PlayDummyCountdown(string startText)
    {
        // Visual Countdown Mandiri khusus untuk Scene Dummie
        ShowCountdown(startText);
        yield return new WaitForSecondsRealtime(0.8f);
        
        for (int i = 3; i >= 1; i--)
        {
            ShowCountdown(i.ToString());
            yield return new WaitForSecondsRealtime(0.7f); // Sesuaikan dengan delay StartCountdownManager
        }
        
        ShowCountdown("GO!");
        yield return new WaitForSecondsRealtime(0.6f);
        HideCountdown();
    }
    #endregion

    #region Crosshair & Score
    public void FlashCrosshairHit(PlayerIndex player)
    {
        if (player == PlayerIndex.Player1)
        {
            if (flashCoroutineP1 != null) StopCoroutine(flashCoroutineP1);
            flashCoroutineP1 = StartCoroutine(CrosshairFlash(crosshairImageP1));
        }
        else
        {
            if (flashCoroutineP2 != null) StopCoroutine(flashCoroutineP2);
            flashCoroutineP2 = StartCoroutine(CrosshairFlash(crosshairImageP2));
        }
    }

    private IEnumerator CrosshairFlash(Image img)
    {
        if (img == null) yield break;
        img.color = crosshairHitColor;
        img.transform.localScale = Vector3.one * 1.4f;
        yield return new WaitForSeconds(0.08f);
        img.color = crosshairDefaultColor;
        img.transform.localScale = Vector3.one;
    }

    private void OnScoreChanged(PlayerIndex player, int newScore, int delta)
    {
        UpdateScore(player, newScore);
        if (delta != 0) FlashCrosshairHit(player);
    }

    public void UpdateScore(PlayerIndex player, int score)
    {
        if (player == PlayerIndex.Player1 && scoreTextP1 != null) scoreTextP1.text = score.ToString("D4");
        else if (player == PlayerIndex.Player2 && scoreTextP2 != null) scoreTextP2.text = score.ToString("D4");
    }

    public void UpdateAccuracy(PlayerIndex player, float accuracy)
    {
        if (player == PlayerIndex.Player1 && accuracyTextP1 != null) accuracyTextP1.text = $"ACC : {accuracy:F0}%";
        else if (player == PlayerIndex.Player2 && accuracyTextP2 != null) accuracyTextP2.text = $"ACC : {accuracy:F0}%";
    }

    public void UpdateTimer(float remaining, float total)
    {
        if (timerText != null)
        {
            int secs = Mathf.CeilToInt(remaining);
            timerText.text = $"{secs:D2}";
            timerText.color = remaining < 15f ? Color.red : Color.white;
        }
        if (timerBarFill != null) timerBarFill.fillAmount = Mathf.Clamp01(remaining / total);
    }

    private void UpdateSpeedDisplay()
    {
        if (speedText == null || CameraController.Instance == null) return;
        speedText.text = $"SPD {CameraController.Instance.CurrentSpeed:F0}";
    }

    public void ShowScorePopup(int delta, Vector3 worldPosition) { }
    #endregion
}
