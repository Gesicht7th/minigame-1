// Assets/_Game/Scripts/SceneFlow/SceneFlowManager.cs
// Singleton yang bertahan antar scene — satu-satunya cara load scene yang aman

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardPunk
{
    public class SceneFlowManager : MonoBehaviour
    {
        public static SceneFlowManager Instance { get; private set; }

        [Header("── Transition Settings ──")]
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private float transitionTime = 0.5f;

        // Key untuk simpan "scene berikutnya" di loading screen
        private const string NEXT_SCENE_KEY = "WP_NextScene";

        void Awake()
        {
            // Jika sudah ada instance lain yang bertahan dari scene sebelumnya,
            // hancurkan yang baru dibuat ini agar tidak terjadi duplikat.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // [PERBAIKAN]
            // Memaksa GameObject ini menjadi Root.
            // DontDestroyOnLoad akan GAGAL (objek tetap hancur) jika objek ini adalah Child dari objek lain.
            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }

        // ── Load langsung (tanpa loading screen) ─────
        public void GoTo(string sceneName)
        {
            Time.timeScale = 1f; // Pastikan waktu berjalan normal
            Debug.Log($"[Flow] → {sceneName}");
            StartCoroutine(LoadWithDelay(sceneName, 0f));
        }

        // ── Load dengan jeda singkat ──────────────────
        public void GoToDelayed(string sceneName, float delay)
        {
            Time.timeScale = 1f; // Pastikan waktu berjalan normal
            Debug.Log($"[Flow] → {sceneName} (delay {delay}s)");
            StartCoroutine(LoadWithDelay(sceneName, delay));
        }

        // ── Load via Loading Screen ────────────────────
        public void GoToViaLoading(string targetScene, float loadingDuration = 2f)
        {
            Time.timeScale = 1f; // Pastikan waktu berjalan normal
            SetNextScene(targetScene);
            SetLoadingDuration(loadingDuration);
            Debug.Log($"[Flow] → LoadingScene → {targetScene}");
            StartCoroutine(LoadWithDelay(SceneNames.LoadingScene, 0f));
        }

        private IEnumerator LoadWithDelay(string sceneName, float delay)
        {
            // Gunakan WaitForSecondsRealtime agar jeda tidak nyangkut jika Time.timeScale sempat 0
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

            // Validasi scene ada di build
            bool found = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName) { found = true; break; }
            }

            if (!found)
            {
                Debug.LogError($"[Flow] Scene '{sceneName}' TIDAK ADA di Build Profiles! " +
                               $"Buka File → Build Profiles dan tambahkan scene tersebut.");
                yield break;
            }

            // --- TAMBAHAN TRANSISI: Mulai animasi menutup layar ---
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger("StartTransition");

                // Gunakan realtime agar transisi tetap jalan meski game di-pause (Time.timeScale = 0)
                yield return new WaitForSecondsRealtime(transitionTime);
            }

            // --- LOAD SCENE ASLI MILIK ANDA ---
            SceneManager.LoadScene(sceneName);

            // Karena script ini DontDestroyOnLoad, Coroutine akan terus hidup.
            // Kita tunggu 1 frame agar Scene baru selesai di-render sebelum membuka layar.
            yield return null;

            // --- TAMBAHAN TRANSISI: Mulai animasi membuka layar di Scene baru ---
            if (transitionAnimator != null)
            {
                transitionAnimator.Play("FadeIn");
            }
        }

        // ── Helper untuk Loading Scene ────────────────
        public void SetNextScene(string scene)
        {
            PlayerPrefs.SetString(NEXT_SCENE_KEY, scene);
            PlayerPrefs.Save();
        }

        public string GetNextScene(string fallback = "MainMenu")
            => PlayerPrefs.GetString(NEXT_SCENE_KEY, fallback);

        public void SetLoadingDuration(float dur)
        {
            PlayerPrefs.SetFloat("WP_LoadDuration", dur);
            PlayerPrefs.Save();
        }

        public float GetLoadingDuration() =>
            PlayerPrefs.GetFloat("WP_LoadDuration", 2f);
    }
}