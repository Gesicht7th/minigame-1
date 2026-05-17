// Assets/_Game/Scripts/MemoryTest/RuneObject.cs
using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class RuneObject : MonoBehaviour
    {
        [SerializeField] private Renderer boxRenderer;

        [Header("── 3D Rune Assets ──")]
        [SerializeField] private GameObject runeUpObj;
        [SerializeField] private GameObject runeDownObj;
        [SerializeField] private GameObject runeLeftObj;
        [SerializeField] private GameObject runeRightObj;

        [Header("── Feedback Light ──")]
        [SerializeField] private Light runeLight; // Pasangkan Point Light di sini

        [Header("Colors")]
        public Color colorIdle = Color.gray;
        public Color colorShow = Color.cyan;
        public Color colorCorrect = Color.green;
        public Color colorWrong = Color.red;

        public WandDirection AssignedDirection { get; private set; }
        private Material mat;
        private Coroutine flipCoroutine;
        private Vector3 initialScale;
        void Awake()
        {
            if (boxRenderer != null) mat = boxRenderer.material;
            if (runeLight != null) runeLight.color = colorIdle;

            // Simpan skala asli saat game dimulai agar kita tahu ukuran maksimalnya
            initialScale = transform.localScale;
        }

        public void Initialize(WandDirection dir)
        {
            AssignedDirection = dir;
            Setup3DRune(); // Aktifkan rune yang sesuai untuk ronde ini
            SetIdle();
        }

        private void Setup3DRune()
        {
            if (runeUpObj != null) runeUpObj.SetActive(AssignedDirection == WandDirection.Up);
            if (runeDownObj != null) runeDownObj.SetActive(AssignedDirection == WandDirection.Down);
            if (runeLeftObj != null) runeLeftObj.SetActive(AssignedDirection == WandDirection.Left);
            if (runeRightObj != null) runeRightObj.SetActive(AssignedDirection == WandDirection.Right);
        }

        public void SetIdle()
        {
            if (mat != null) mat.color = colorIdle;
            if (runeLight != null) runeLight.color = colorIdle;

            // Sesuai request: Di awal ronde/idle, posisi default adalah MENGHADAP BELAKANG (180 derajat)
            transform.localRotation = Quaternion.identity;
        }

        public void ShowArrow()
        {
            if (mat != null) mat.color = colorShow;
            if (runeLight != null) runeLight.color = colorShow;

            // Berputar ke DEPAN (0 derajat) untuk memperlihatkan arah ke player
            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.Euler(0f, 180f, 0f)));
        }

        public void HideArrow()
        {
            if (mat != null) mat.color = colorIdle;
            if (runeLight != null) runeLight.color = colorIdle;

            // Berputar kembali ke BELAKANG (180 derajat) untuk menyembunyikan arah
            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.identity));
        }

        public void ShowResult(bool correct)
        {
            if (mat != null) mat.color = correct ? colorCorrect : colorWrong;
            if (runeLight != null) runeLight.color = correct ? colorCorrect : colorWrong;

            if (correct)
            {
                // Jika BENAR: Berputar menghadap DEPAN (0 derajat)
                if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.Euler(0f, 180f, 0f)));
            }
            else
            {
                // Jika SALAH: Tetap menghadap BELAKANG (180 derajat), warna lampu otomatis jadi merah lewat baris di atas
                if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                transform.localRotation = Quaternion.identity;
            }
        }
        public void AnimateToIdle()
        {
            if (mat != null) mat.color = colorIdle;
            if (runeLight != null) runeLight.color = colorIdle;

            // Putar balik ke 180 derajat (menghadap belakang) dengan animasi smooth
            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.identity));
        }
        private IEnumerator FlipAnimation(Quaternion startRot, Quaternion targetRot)
        {
            float elapsed = 0f;
            float duration = 0.25f; // Sedikit dicepatkan agar transisinya terasa responsif dan ritmis

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localRotation = Quaternion.Lerp(startRot, targetRot, elapsed / duration);
                yield return null;
            }

            transform.localRotation = targetRot;
        }

        public void PlayAppearAnimation()
        {
            StartCoroutine(ScaleUpAnimation());
        }

        private IEnumerator ScaleUpAnimation()
        {
            float elapsed = 0f;
            float duration = 0.2f; // Durasi membesar

            // Set ukuran menjadi 0 di awal animasi
            transform.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Gunakan SmoothStep agar animasinya melambat di akhir (tidak kaku)
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t);

                yield return null;
            }

            // Pastikan presisi ukuran di akhir animasi
            transform.localScale = initialScale;
        }
    }
}