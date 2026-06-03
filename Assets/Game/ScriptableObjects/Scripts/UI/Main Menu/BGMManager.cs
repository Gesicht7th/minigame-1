// Assets/_Game/Scripts/Audio/BGMManager.cs
using UnityEngine;

namespace WizardPunk
{
    public class BGMManager : MonoBehaviour
    {
        public static BGMManager Instance;

        [SerializeField] private AudioSource bgmSource;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                    bgmSource.playOnAwake = false;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayBGM(AudioClip newBgm)
        {
            if (newBgm == null) return;

            if (bgmSource.clip == newBgm && bgmSource.isPlaying) return;

            bgmSource.clip = newBgm;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }
    }
}