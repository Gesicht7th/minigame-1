// Assets/_Game/Scripts/Audio/SceneBGM.cs
using UnityEngine;

namespace WizardPunk
{
    public class SceneBGM : MonoBehaviour
    {
        [Header("── Background Music ──")]
        [Tooltip("Masukkan file audio lagu untuk scene ini")]
        [SerializeField] private AudioClip bgmClip;

        void Start()
        {
            // Panggil SoundManager global untuk memutar lagu ini saat scene dimulai
            if (SoundManager.Instance != null && bgmClip != null)
            {
                SoundManager.Instance.PlayBGM(bgmClip);
            }
            else if (SoundManager.Instance == null)
            {
                Debug.LogWarning("SoundManager belum ada di scene! Pastikan masuk dari Main Menu.");
            }
        }
    }
}