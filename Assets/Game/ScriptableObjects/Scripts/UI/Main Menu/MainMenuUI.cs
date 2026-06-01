// Assets/_Game/Scripts/UI/MainMenuUI.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button playButton;

        [Header("── Tutorial Panel ──────────────────────")]
        [SerializeField] private GameObject tutorialPanel;

        [Tooltip("String yang dikirim ESP32 saat tombol Action ditekan (misal: 'B', 'ACTION', atau 'F')")]
        [SerializeField] private string espButtonCode = "B";

        [Header("── Optional Ready Indicators ───────────")]
        [Tooltip("Opsional: Objek centang/teks yang menyala jika P1 sudah tekan tombol")]
        [SerializeField] private GameObject p1ReadyIndicator;
        [Tooltip("Opsional: Objek centang/teks yang menyala jika P2 sudah tekan tombol")]
        [SerializeField] private GameObject p2ReadyIndicator;

        [Header("── ESP32 Readers ───────────────────────")]
        [SerializeField] private WandSerialReader p1SerialReader;
        [SerializeField] private WandSerialReader p2SerialReader;

        [Header("── High Score Texts ────────────────────")]
        [SerializeField] private TextMeshProUGUI hs_mg1Text;
        [SerializeField] private TextMeshProUGUI hs_mg2Text;
        [SerializeField] private TextMeshProUGUI hs_mg3Text;

        private bool isWaitingForPlayers = false;

        // --- VARIABEL UNTUK MENYIMPAN UKURAN ASLI ---
        private Vector3 p1IndicatorOriginalScale = Vector3.one;
        private Vector3 p2IndicatorOriginalScale = Vector3.one;

        void Start()
        {
            // Auto-find Serial Readers
            if (p1SerialReader == null || p2SerialReader == null)
            {
                WandSerialReader[] readers = FindObjectsOfType<WandSerialReader>();
                foreach (var reader in readers)
                {
                    if (reader.name.ToUpper().Contains("P1") || reader.name.ToUpper().Contains("PLAYER1"))
                        p1SerialReader = reader;
                    else if (reader.name.ToUpper().Contains("P2") || reader.name.ToUpper().Contains("PLAYER2"))
                        p2SerialReader = reader;
                }
            }

            // Tampilkan high score
            if (hs_mg1Text)
                hs_mg1Text.text = $"Memory Test : {PlayerPrefs.GetInt("MemoryTest_HighScore", 0)}";
            if (hs_mg2Text)
                hs_mg2Text.text = $"Hyper Smash : {PlayerPrefs.GetInt("HyperSmash_HighScore", 0)}";
            if (hs_mg3Text)
                hs_mg3Text.text = $"Reflex P1 : {PlayerPrefs.GetInt("Reflex_P1_HighScore", 0)}" +
                                  $"  P2 : {PlayerPrefs.GetInt("Reflex_P2_HighScore", 0)}";

            // Simpan ukuran asli indikator dari Inspector sebelum disembunyikan
            if (p1ReadyIndicator != null) p1IndicatorOriginalScale = p1ReadyIndicator.transform.localScale;
            if (p2ReadyIndicator != null) p2IndicatorOriginalScale = p2ReadyIndicator.transform.localScale;

            // Sembunyikan tutorial panel dan indikator di awal
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (p1ReadyIndicator != null) p1ReadyIndicator.SetActive(false);
            if (p2ReadyIndicator != null) p2ReadyIndicator.SetActive(false);

            // Tombol PLAY → Tampilkan Tutorial & Tunggu Input
            playButton?.onClick.AddListener(() =>
            {
                if (tutorialPanel != null)
                {
                    tutorialPanel.SetActive(true);
                    if (!isWaitingForPlayers)
                    {
                        StartCoroutine(WaitForPlayersReady());
                    }
                }
                else
                {
                    StartGame();
                }
            });
        }

        private IEnumerator WaitForPlayersReady()
        {
            isWaitingForPlayers = true;
            bool p1Ready = false;
            bool p2Ready = false;

            p1SerialReader?.FlushGesture();
            p2SerialReader?.FlushGesture();

            while (!p1Ready || !p2Ready)
            {
                // Cek Player 1
                if (!p1Ready)
                {
                    if (Input.GetKeyDown(KeyCode.Space) ||
                       (p1SerialReader != null && p1SerialReader.ConsumeGesture() == espButtonCode))
                    {
                        p1Ready = true;

                        // Mainkan Animasi dengan patokan skala asli P1
                        if (p1ReadyIndicator != null)
                            StartCoroutine(AnimateIndicator(p1ReadyIndicator, p1IndicatorOriginalScale));

                        if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(null);
                    }
                }

                // Cek Player 2
                if (!p2Ready)
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                       (p2SerialReader != null && p2SerialReader.ConsumeGesture() == espButtonCode))
                    {
                        p2Ready = true;

                        // Mainkan Animasi dengan patokan skala asli P2
                        if (p2ReadyIndicator != null)
                            StartCoroutine(AnimateIndicator(p2ReadyIndicator, p2IndicatorOriginalScale));

                        if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(null);
                    }
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.6f);
            StartGame();
        }

        // --- FUNGSI ANIMASI YANG SUDAH DIPERBAIKI ---
        private IEnumerator AnimateIndicator(GameObject indicator, Vector3 originalScale)
        {
            indicator.transform.localScale = Vector3.zero;
            indicator.SetActive(true);

            // Tentukan batas membesar (20% lebih besar dari ukuran aslinya, bukan 1.2 absolut)
            Vector3 popScale = originalScale * 1.2f;

            float duration = 0.2f;
            float elapsed = 0f;

            // Membesar sedikit melebih ukuran asli
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                indicator.transform.localScale = Vector3.Lerp(Vector3.zero, popScale, t);
                yield return null;
            }

            // Mengecil kembali ke ukuran asli yang sebenarnya
            duration = 0.1f;
            elapsed = 0f;
            Vector3 currentScale = indicator.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                indicator.transform.localScale = Vector3.Lerp(currentScale, originalScale, t);
                yield return null;
            }

            // Kunci rapat di ukuran aslinya
            indicator.transform.localScale = originalScale;
        }
        // --------------------------------------------

        private void StartGame()
        {
            GameDataBridge.Instance?.ResetAll();
            SceneFlowManager.Instance.GoTo(SceneNames.ReadyScreen);
        }
    }
}