// Assets/_Game/Scripts/Core/AudioManager.cs

using System.Collections.Generic;
using UnityEngine;

namespace WizardPunk
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }
        #endregion

        #region SFX Keys (konstanta — tidak typo)
        public const string SFX_CORRECT = "correct";
        public const string SFX_WRONG = "wrong";
        public const string SFX_COUNTDOWN = "countdown";
        public const string SFX_GO = "go";
        public const string SFX_PATTERN_SHOW = "pattern_show";
        public const string SFX_GAMEOVER = "gameover";
        public const string BGM_MAIN = "bgm_main";
        #endregion

        #region Data
        [System.Serializable]
        public class AudioEntry
        {
            public string key;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            public bool randomizePitch = false;
            [Range(0.8f, 1.2f)] public float pitchVariation = 0.1f;
        }
        #endregion

        #region Inspector Fields
        [Header("── Audio Entries ───────────────────────")]
        [SerializeField] private List<AudioEntry> sfxEntries = new();
        [SerializeField] private List<AudioEntry> bgmEntries = new();

        [Header("── Volume ───────────────────────────────")]
        [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.6f;
        #endregion

        #region Private Fields
        private AudioSource sfxSource;
        private AudioSource bgmSource;
        private Dictionary<string, AudioEntry> sfxDict = new();
        private Dictionary<string, AudioEntry> bgmDict = new();
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            foreach (var e in sfxEntries) if (!string.IsNullOrEmpty(e.key)) sfxDict[e.key] = e;
            foreach (var e in bgmEntries) if (!string.IsNullOrEmpty(e.key)) bgmDict[e.key] = e;
        }
        #endregion

        #region Public Methods
        public void PlaySFX(string key)
        {
            if (!sfxDict.TryGetValue(key, out var entry))
            {
                Debug.LogWarning($"[Audio] SFX key tidak ditemukan: '{key}'");
                return;
            }
            if (entry.clip == null) return;

            float vol = entry.volume * sfxVolume * masterVolume;
            if (entry.randomizePitch)
            {
                sfxSource.pitch = 1f + Random.Range(-entry.pitchVariation, entry.pitchVariation);
            }
            sfxSource.PlayOneShot(entry.clip, vol);
        }

        public void PlayBGM(string key)
        {
            if (!bgmDict.TryGetValue(key, out var entry))
            {
                Debug.LogWarning($"[Audio] BGM key tidak ditemukan: '{key}'");
                return;
            }
            if (entry.clip == null) return;

            bgmSource.clip = entry.clip;
            bgmSource.volume = entry.volume * bgmVolume * masterVolume;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();
        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); }
        public void SetBGMVolume(float v) { bgmVolume = Mathf.Clamp01(v); bgmSource.volume = bgmVolume * masterVolume; }
        #endregion
    }
}