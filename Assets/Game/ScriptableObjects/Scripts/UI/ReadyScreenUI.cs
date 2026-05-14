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
        [SerializeField] private Image unreadyImage1;
        [SerializeField] private Image readyImage1;
        [SerializeField] private TextMeshProUGUI p1StatusText;

        [Header("── Layar Pemain 2 (Kanan) ──────────────")]
        [SerializeField] private Image unreadyImage2;
        [SerializeField] private Image readyImage2;
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
            {
                p1Ready = !p1Ready;
                UpdateIndicators(); // Perbarui tampilan dulu agar gambar aktif

                // Jika berubah menjadi siap, jalankan animasi pop up
                if (p1Ready && readyImage1 != null)
                {
                    StartCoroutine(PopAnimation(readyImage1.transform));
                }
            }

            // Pemain 2 siap: Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                p2Ready = !p2Ready;
                UpdateIndicators(); // Perbarui tampilan dulu agar gambar aktif

                // Jika berubah menjadi siap, jalankan animasi pop up
                if (p2Ready && readyImage2 != null)
                {
                    StartCoroutine(PopAnimation(readyImage2.transform));
                }
            }

            // Escape = batal semua
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                p1Ready = false;
                p2Ready = false;
                UpdateIndicators();
            }

            // Kedua pemain siap → mulai hitung mundur
            if (p1Ready && p2Ready && !startingGame)
            {
                startingGame = true;
                StartCoroutine(StartCountdown());
            }
        }

        private void UpdateIndicators()
        {
            // Aktif/Nonaktifkan Image Pemain 1
            if (unreadyImage1 != null) unreadyImage1.gameObject.SetActive(!p1Ready);
            if (readyImage1 != null) readyImage1.gameObject.SetActive(p1Ready);

            // Aktif/Nonaktifkan Image Pemain 2
            if (unreadyImage2 != null) unreadyImage2.gameObject.SetActive(!p2Ready);
            if (readyImage2 != null) readyImage2.gameObject.SetActive(p2Ready);

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
                    UpdateIndicators(); // Memastikan UI langsung diperbarui jika dibatalkan
                    yield break;
                }
            }

            // Mulai permainan
            SceneFlowManager.Instance.GoTo(SceneNames.MemoryTest);
        }

        // --- Fungsi Animasi Pop Up ---
        private IEnumerator PopAnimation(Transform target)
        {
            // Set ukuran awal menjadi 0 (tidak terlihat)
            target.localScale = Vector3.zero;

            float timer = 0f;
            float durationUp = 0.15f; // Waktu untuk membesar melebihi ukuran asli
            float durationDown = 0.1f; // Waktu untuk kembali ke ukuran normal

            // Fase 1: Membesar dari 0 ke 1.2
            while (timer < durationUp)
            {
                timer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1.2f, timer / durationUp);
                target.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // Fase 2: Mengecil dari 1.2 kembali ke ukuran 1
            timer = 0f;
            while (timer < durationDown)
            {
                timer += Time.deltaTime;
                float scale = Mathf.Lerp(1.2f, 1f, timer / durationDown);
                target.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // Pastikan ukurannya tepat di akhir animasi
            target.localScale = Vector3.one;
        }
    }
}