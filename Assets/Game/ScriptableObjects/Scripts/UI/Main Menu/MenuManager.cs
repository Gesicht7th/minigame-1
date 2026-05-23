// Assets/_Game/Scripts/UI/MenuManager.cs
using UnityEngine;

namespace WizardPunk
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject optionsPanel;

        void Start()
        {
            // Pastikan saat game mulai, menu utama aktif dan options mati
            mainMenuPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }

        public void OpenOptions()
        {
            mainMenuPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }

        public void CloseOptions()
        {
            optionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }
}