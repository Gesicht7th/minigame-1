using UnityEngine;
using System.Collections;
using WizardPunk.HyperSmash; // Akses ke CameraController bawaan Anda

public class StartCountdownManager : MonoBehaviour
{
    [Header("Pengaturan Hitung Mundur")]
    public float countdownDelay = 3.5f; 

    void Start()
    {
        // Matikan pergerakan kamera secara resmi menggunakan script bawaan Anda!
        // Ini menghindari bug tabrakan/physics ("aset terkena push").
        if (CameraController.Instance != null)
        {
            CameraController.Instance.StopMoving();
        }

        StartCoroutine(PauseForCountdown());
    }

    IEnumerator PauseForCountdown()
    {
        // Waktu game tidak perlu di-set ke 0 agar tidak mengganggu UI
        // Cukup tunggu hitung mundur UI selesai
        yield return new WaitForSeconds(countdownDelay);

        // Setelah selesai, suruh kamera bergerak lagi!
        if (CameraController.Instance != null)
        {
            CameraController.Instance.StartMoving();
        }

        Debug.Log("[Countdown] Selesai! Kamera mulai bergerak.");
    }
}
