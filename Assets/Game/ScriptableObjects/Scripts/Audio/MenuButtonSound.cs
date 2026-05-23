using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WizardPunk
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonSound : MonoBehaviour, IPointerClickHandler
    {
        [Header("── Audio Settings ──────────────────────")]
        [SerializeField] private AudioClip selectClip;
        [SerializeField] private AudioClip backClip;
        [SerializeField] private bool isBackButton = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            // Memanggil fungsi dari SoundManager pusat
            if (SoundManager.Instance != null)
            {
                if (isBackButton && backClip != null)
                    SoundManager.Instance.PlaySound(backClip);
                else if (!isBackButton && selectClip != null)
                    SoundManager.Instance.PlaySound(selectClip);
            }
        }
    }
}