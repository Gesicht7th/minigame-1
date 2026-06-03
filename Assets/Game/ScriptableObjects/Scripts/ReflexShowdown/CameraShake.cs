using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ShakeProfile
{
    public string profileName = "Default";

    [Header("Timings (Waktu Tunggu)")]
    [Tooltip("Waktu tunggu sebelum FOV mulai menukik (dalam detik)")]
    public float delayBeforePunch = 0f;
    [Tooltip("Waktu tunggu sebelum layar mulai bergetar (dalam detik)")]
    public float delayBeforeShake = 0.05f;

    [Header("FOV Punch Settings")]
    public bool useFovPunch = true;
    public float targetFov = 45f;
    public float fovDropDuration = 0.05f;
    public float fovRestoreSpeed = 10f;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    public float dampingSpeed = 1.0f;
}

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private List<ShakeProfile> profiles = new List<ShakeProfile>();

    private Vector3 originalPos;
    private float currentShakeDuration = 0f;
    private float currentShakeMagnitude = 0f;
    private float initialDuration = 0f;
    private float activeDampingSpeed = 1f;

    private Camera cam;
    private float originalFov;
    
    private Coroutine activeRoutine;
    private bool isRestoringFov = false;
    private float activeRestoreSpeed = 10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = GetComponent<Camera>();
        
        // Otomatis buat profil standar jika list kosong
        if (profiles.Count == 0)
        {
            profiles.Add(new ShakeProfile { profileName = "Win" });
            profiles.Add(new ShakeProfile { profileName = "Draw" });
        }
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
        if (cam != null) originalFov = cam.fieldOfView;
    }

    private void Update()
    {
        // Restore FOV smooth ke ukuran aslinya
        if (isRestoringFov && cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, originalFov, Time.deltaTime * activeRestoreSpeed);
            if (Mathf.Abs(cam.fieldOfView - originalFov) < 0.1f)
            {
                cam.fieldOfView = originalFov;
                isRestoringFov = false;
            }
        }

        // Logika Shake
        if (currentShakeDuration > 0)
        {
            float dampingFactor = currentShakeDuration / initialDuration; 
            float currentMag = currentShakeMagnitude * dampingFactor;
            
            transform.localPosition = originalPos + Random.insideUnitSphere * currentMag;
            
            currentShakeDuration -= Time.deltaTime * activeDampingSpeed;
        }
        else
        {
            currentShakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }

    /// <summary>
    /// Memanggil getaran berdasarkan nama profile. Jika tidak ada, akan mencoba fallbackProfileName.
    /// </summary>
    public void TriggerProfile(string profileName, string fallbackProfileName = "")
    {
        ShakeProfile profile = profiles.Find(p => p.profileName == profileName);
        
        if (profile == null && !string.IsNullOrEmpty(fallbackProfileName))
        {
            profile = profiles.Find(p => p.profileName == fallbackProfileName);
        }

        if (profile != null)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(ExecuteProfile(profile));
        }
        else
        {
            Debug.LogWarning("CameraShake: Profile '" + profileName + "' tidak ditemukan!");
        }
    }

    private IEnumerator ExecuteProfile(ShakeProfile profile)
    {
        float timer = 0f;
        bool punchExecuted = !profile.useFovPunch;
        bool shakeExecuted = false;

        isRestoringFov = false;
        activeRestoreSpeed = profile.fovRestoreSpeed;
        activeDampingSpeed = profile.dampingSpeed;

        while (!punchExecuted || !shakeExecuted)
        {
            // Cek apakah sudah waktunya FOV Punch
            if (!punchExecuted && timer >= profile.delayBeforePunch)
            {
                punchExecuted = true;
                StartCoroutine(DoFovPunch(profile));
            }

            // Cek apakah sudah waktunya Shake
            if (!shakeExecuted && timer >= profile.delayBeforeShake)
            {
                shakeExecuted = true;
                StartShake(profile.shakeDuration, profile.shakeMagnitude);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DoFovPunch(ShakeProfile profile)
    {
        float elapsed = 0f;
        float startFov = cam.fieldOfView;

        while (elapsed < profile.fovDropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / profile.fovDropDuration;
            cam.fieldOfView = Mathf.Lerp(startFov, profile.targetFov, t);
            yield return null;
        }

        cam.fieldOfView = profile.targetFov;
        isRestoringFov = true; // Izinkan Update() untuk menaikkan FOV secara halus
    }

    private void StartShake(float duration, float magnitude)
    {
        initialDuration = Mathf.Max(duration, 0.01f);
        currentShakeDuration = initialDuration;
        currentShakeMagnitude = magnitude;
    }

    [ContextMenu("Test Profile Pertama")]
    public void TestFirstProfile()
    {
        if (profiles.Count > 0) TriggerProfile(profiles[0].profileName);
    }
}
