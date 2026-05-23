using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public class HyperSmashSoundController : MonoBehaviour
    {
        public static HyperSmashSoundController Instance;

        [Header("── In-Game Audio Clips ──────────────────")]
        [SerializeField] private AudioClip p1ShootSFX;
        [SerializeField] private AudioClip p2ShootSFX;
        [SerializeField] private AudioClip crystalDestroySFX;

        [Header("── UI Event Audio Clips ─────────────────")]
        [SerializeField] private AudioClip timesUpSFX;
        [SerializeField] private AudioClip resultSFX;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayP1ShootSound()
        {
            if (SoundManager.Instance != null && p1ShootSFX != null)
                SoundManager.Instance.PlaySound(p1ShootSFX);
        }

        public void PlayP2ShootSound()
        {
            if (SoundManager.Instance != null && p2ShootSFX != null)
                SoundManager.Instance.PlaySound(p2ShootSFX);
        }

        public void PlayCrystalDestroySound()
        {
            if (SoundManager.Instance != null && crystalDestroySFX != null)
                SoundManager.Instance.PlaySound(crystalDestroySFX);
        }

        public void PlayTimesUpSound()
        {
            if (SoundManager.Instance != null && timesUpSFX != null)
                SoundManager.Instance.PlaySound(timesUpSFX);
        }

        public void PlayResultSound()
        {
            if (SoundManager.Instance != null && resultSFX != null)
                SoundManager.Instance.PlaySound(resultSFX);
        }
    }
}