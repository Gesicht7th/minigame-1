using UnityEngine;
using UnityEngine.EventSystems; // Tambahkan ini untuk mendeteksi hover UI

public class UIPulseEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Kecepatan membesar dan mengecil")]
    public float pulseSpeed = 5f;
    
    [Tooltip("Seberapa besar perubahannya. 0.1 berarti 10% lebih besar/kecil")]
    public float pulseScale = 0.1f;
    
    [Tooltip("Perbesaran ukuran tombol ketika di-hover mouse")]
    public float hoverScaleMultiplier = 1.15f;
    
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    // Menggunakan LateUpdate agar dapat menimpa (override) animasi bawaan dari Button jika ada
    void LateUpdate()
    {
        if (isHovered)
        {
            // Saat di-hover, tombol membesar secara konstan dan diam
            Vector3 targetScale = originalScale * hoverScaleMultiplier;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 15f);
        }
        else
        {
            // Menggunakan perkalian agar perubahannya proporsional dengan ukuran aslinya (berfungsi di scale yang tidak 1,1,1)
            float pulseMultiplier = 1f + (Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale);
            transform.localScale = originalScale * pulseMultiplier;
        }
    }

    // Dipanggil otomatis saat mouse masuk ke area tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    // Dipanggil otomatis saat mouse keluar dari area tombol
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
