// Assets/_Game/Scripts/UI/ReadyScreenUI.cs

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class ReadyScreenUI : MonoBehaviour
    {
        [Header("── Layar Pemain 1 (Kiri) ───────────────")]
        [SerializeField] private Sprite unreadyScreen1;
        [SerializeField] private Sprite readyScreen1;
        [SerializeField] private Image p1ImageUI; // Komponen Image target di Canvas
        [SerializeField] private TextMeshProUGUI p1StatusText;

        [Header("── Layar Pemain 2 (Kanan) ──────────────")]
        [SerializeField] private Sprite unreadyScreen2;
        [SerializeField] private Sprite readyScreen2;
        [SerializeField] private Image p2ImageUI; // Komponen Image target di Canvas
        [SerializeField] private TextMeshProUGUI p2StatusText;

        [Header("── Teks Utama ──────────────────────────")]
        [SerializeField] private TextMeshProUGUI bothReadyText;

        [Header("── Tombol ──────────────────────────────")]
        [SerializeField] private Button backButton;

        [Header("── Hitung Mundur ───────────────────────")]
        [SerializeField] private float countdownBeforeStart = 3f;

        // Status
        private bool p1Ready = false;
        private bool p2Ready = false;
        private bool startingGame = false;

        void Start()
        {
            UpdateIndicators();
            if (bothReadyText != null) bothReadyText.gameObject.SetActive(false);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                    SceneFlowManager.Instance.GoTo(SceneNames.MainMenu));
            }
        }

        void Update()
        {
            if (startingGame) return;

            // Pemain 1 siap: Spasi
            if (Input.GetKeyDown(KeyCode.Space))
                p1Ready = !p1Ready;

            // Pemain 2 siap: Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                p2Ready = !p2Ready;

            // Escape = batal semua
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                p1Ready = false;
                p2Ready = false;
            }

            UpdateIndicators();

            // Kedua pemain siap → mulai hitung mundur
            if (p1Ready && p2Ready && !startingGame)
            {
                startingGame = true;
                StartCoroutine(StartCountdown());
            }
        }

        private void UpdateIndicators()
        {
            // Perbarui gambar Pemain 1
            if (p1ImageUI != null) 
                p1ImageUI.sprite = p1Ready ? readyScreen1 : unreadyScreen1;

            // Perbarui gambar Pemain 2
            if (p2ImageUI != null) 
                p2ImageUI.sprite = p2Ready ? readyScreen2 : unreadyScreen2;

            // Perbarui teks Pemain 1
            if (p1StatusText != null) 
            {
                p1StatusText.text = p1Ready ? "READY" : "NOT READY";
                p1StatusText.color = p1Ready ? Color.green : Color.red; 
            }

            // Perbarui teks Pemain 2
            if (p2StatusText != null) 
            {
                p2StatusText.text = p2Ready ? "READY" : "NOT READY";
                p2StatusText.color = p2Ready ? Color.green : Color.red; 
            }
        }

        private IEnumerator StartCountdown()
        {
            if (bothReadyText != null) bothReadyText.gameObject.SetActive(true);

            for (int i = (int)countdownBeforeStart; i >= 1; i--)
            {
                if (bothReadyText != null) bothReadyText.text = $"Starting in {i}...";
                yield return new WaitForSeconds(1f);

                // Jika salah satu pemain membatalkan status siap
                if (!p1Ready || !p2Ready)
                {
                    startingGame = false;
                    if (bothReadyText != null) bothReadyText.gameObject.SetActive(false);
                    yield break;
                }
            }

            // Mulai permainan
            SceneFlowManager.Instance.GoTo(SceneNames.MemoryTest);
        }
    }
}