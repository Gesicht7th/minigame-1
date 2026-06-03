using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace WizardPunk
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private AudioMixer myMixer;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        void Start()
        {
            // Ambil data dari PlayerPrefs agar volume tersimpan meski game ditutup
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            // Terapkan saat game dimulai
            UpdateMusicVolume(musicSlider.value);
            UpdateSFXVolume(sfxSlider.value);
        }

        public void UpdateMusicVolume(float value)
        {
            // Ubah nilai 0-1 menjadi skala Logaritmik (dB)
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            myMixer.SetFloat("MusicVol", db);
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void UpdateSFXVolume(float value)
        {
            // Ubah nilai 0-1 menjadi skala Logaritmik (dB)
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            myMixer.SetFloat("SFXVol", db);
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }
}