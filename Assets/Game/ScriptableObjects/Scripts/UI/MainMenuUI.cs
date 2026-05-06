// Assets/_Game/Scripts/UI/MainMenuUI.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardPunk
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("── Buttons ─────────────────────────────")]
        [SerializeField] private Button playButton;

        [Header("── High Score Texts ────────────────────")]
        [SerializeField] private TextMeshProUGUI hs_mg1Text;
        [SerializeField] private TextMeshProUGUI hs_mg2Text;
        [SerializeField] private TextMeshProUGUI hs_mg3Text;

        void Start()
        {
            // Tampilkan high score
            if (hs_mg1Text)
                hs_mg1Text.text = $"Memory Test : {PlayerPrefs.GetInt("MemoryTest_HighScore", 0)}";
            if (hs_mg2Text)
                hs_mg2Text.text = $"Hyper Smash : {PlayerPrefs.GetInt("HyperSmash_HighScore", 0)}";
            if (hs_mg3Text)
                hs_mg3Text.text = $"Reflex P1 : {PlayerPrefs.GetInt("Reflex_P1_HighScore", 0)}" +
                                  $"  P2 : {PlayerPrefs.GetInt("Reflex_P2_HighScore", 0)}";

            // Tombol PLAY → ke ReadyScreen
            playButton?.onClick.AddListener(() =>
            {
                // Reset skor lama sebelum game baru
                GameDataBridge.Instance?.ResetAll();
                SceneFlowManager.Instance.GoTo(SceneNames.ReadyScreen);
            });
        }
    }
}