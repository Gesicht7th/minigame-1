using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    // Fungsi untuk membuka Options
    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false); // Sembunyikan menu utama
        optionsPanel.SetActive(true);   // Munculkan pop-up options
    }

    // Fungsi untuk kembali (Klik tombol Back)
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);  // Sembunyikan pop-up options
        mainMenuPanel.SetActive(true);  // Munculkan kembali menu utama
    }
}