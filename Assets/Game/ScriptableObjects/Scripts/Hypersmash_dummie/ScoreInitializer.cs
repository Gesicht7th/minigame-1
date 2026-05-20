using UnityEngine;
using WizardPunk.HyperSmash;

public class ScoreInitializer : MonoBehaviour
{
    void Start()
    {
        // Secara otomatis me-reset skor UI saat game baru dimulai
        if (HyperSmashScoreManager.Instance != null)
        {
            HyperSmashScoreManager.Instance.ResetAll();
        }
    }
}
