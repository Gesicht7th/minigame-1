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
        [SerializeField] private Light runeLight;

        [Header("── Audio ──")]
        [SerializeField] private AudioSource runeSfxSource;
        [SerializeField] private AudioClip flipSound;

        [Header("── Particles ──")]
        [SerializeField] private GameObject correctParticlePrefab;
        [SerializeField] private GameObject wrongParticlePrefab;

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
            initialScale = transform.localScale;
        }

        public void Initialize(WandDirection dir)
        {
            AssignedDirection = dir;
            Setup3DRune();
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
            transform.localRotation = Quaternion.identity;
        }

        // === UPDATE AUDIO ===
        private void PlayFlipSound()
        {
            // 1. Panggil Global Sound untuk Batu Bergerak
            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance.PlayStoneMoveSound();
            }

            // 2. Tetap jalankan local audio sebagai fallback / variasi pitch
            if (runeSfxSource != null && flipSound != null)
            {
                runeSfxSource.pitch = Random.Range(0.9f, 1.1f);
                runeSfxSource.PlayOneShot(flipSound);
            }
        }
        // ====================

        public void ShowArrow()
        {
            if (mat != null) mat.color = colorShow;
            if (runeLight != null) runeLight.color = colorShow;

            // PANGGIL SUARA SAAT BATU MEMBUKA (BERGERAK)
            PlayFlipSound();

            // === PANGGIL SUARA SAAT PANAH MUNCUL ===
            if (MemoryTestSoundController.Instance != null)
            {
                MemoryTestSoundController.Instance.PlayArrowAppearSound();
            }
            // =======================================

            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.Euler(0f, 180f, 0f)));
        }

        public void HideArrow()
        {
            if (mat != null) mat.color = colorIdle;
            if (runeLight != null) runeLight.color = colorIdle;

            PlayFlipSound();

            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.identity));
        }

        public void ShowResult(bool correct)
        {
            if (mat != null) mat.color = correct ? colorCorrect : colorWrong;
            if (runeLight != null) runeLight.color = correct ? colorCorrect : colorWrong;

            if (correct)
            {
                PlayFlipSound();
                
                if (correctParticlePrefab != null)
                {
                    Instantiate(correctParticlePrefab, transform.position, Quaternion.Euler(-90,0,0));
                }

                if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.Euler(0f, 180f, 0f)));
            }
            else
            {
                if (wrongParticlePrefab != null)
                {
                    Instantiate(wrongParticlePrefab, transform.position, Quaternion.Euler(-90, 0, 0));
                }

                if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                transform.localRotation = Quaternion.identity;
            }
        }

        public void AnimateToIdle()
        {
            if (mat != null) mat.color = colorIdle;
            if (runeLight != null) runeLight.color = colorIdle;

            PlayFlipSound();

            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(FlipAnimation(transform.localRotation, Quaternion.identity));
        }

        private IEnumerator FlipAnimation(Quaternion startRot, Quaternion targetRot)
        {
            float elapsed = 0f;
            float duration = 0.25f;

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
            float duration = 0.2f;

            transform.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t);
                yield return null;
            }

            transform.localScale = initialScale;
        }
    }
}