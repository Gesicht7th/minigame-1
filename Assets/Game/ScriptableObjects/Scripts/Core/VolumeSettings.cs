using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace WizardPunk
{
    public class VolumeSettings : MonoBehaviour
    {
        [Header("── Referensi ────────────────────────")]
        [SerializeField] private AudioMixer myMixer;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private void Start()
        {
            // Cek apakah pemain sudah pernah mengatur volume sebelumnya
            if (PlayerPrefs.HasKey("musicVolume"))
            {
                LoadVolume();
            }
            else
            {
                // Jika baru pertama kali main, set volume ke default (slider di posisi saat ini)
                SetMusicVolume();
                SetSFXVolume();
            }
        }

        // Fungsi ini akan dipanggil otomatis saat Slider Musik digeser
        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            // Rumus Logaritma: mengubah nilai slider linier menjadi nilai Desibel (dB) AudioMixer
            myMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("musicVolume", volume);
            PlayerPrefs.Save();
        }

        // Fungsi ini akan dipanggil otomatis saat Slider SFX digeser
        public void SetSFXVolume()
        {
            float volume = sfxSlider.value;
            myMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("sfxVolume", volume);
            PlayerPrefs.Save();
        }

        private void LoadVolume()
        {
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
            sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

            SetMusicVolume();
            SetSFXVolume();
        }
    }
}