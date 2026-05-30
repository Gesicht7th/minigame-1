using UnityEngine;
using WizardPunk.HyperSmash; // Memanggil namespace dari sistem lama Anda

public class DualCrosshairController : MonoBehaviour
{
    [Header("Crosshair 1 (WASD)")]
    public RectTransform crosshair1;
    public float speed1 = 500f;
    public KeyCode shootKey1 = KeyCode.Space; 
    public Animator animator1;
    [Tooltip("Masukan object 'AimFura' (Transform) di sini, BUKAN character utamanya")]
    public Transform aimTarget1;
    public float aimMoveSpeed1 = 15f;
    [Tooltip("Faktor skala pergerakan Aim Target terhadap Crosshair")]
    public float aimScale1 = 0.01f;

    [Header("Crosshair 2 (Arrow Keys)")]
    public RectTransform crosshair2;
    public float speed2 = 500f;
    public KeyCode shootKey2 = KeyCode.RightControl; 
    public Animator animator2;
    [Tooltip("Masukan object 'AimOura' (Transform) di sini, BUKAN character utamanya")]
    public Transform aimTarget2;
    public float aimMoveSpeed2 = 15f;
    [Tooltip("Faktor skala pergerakan Aim Target terhadap Crosshair")]
    public float aimScale2 = 0.01f;

    [Header("Character Model Settings")]
    [Tooltip("Masukan object 'SAM-Fura' (Transform model karakter) di sini")]
    public Transform characterModel1;
    [Tooltip("Masukan object 'SAM-Oura' (Transform model karakter) di sini")]
    public Transform characterModel2;
    [Tooltip("Seberapa besar model karakter ikut bergerak? (0.15 = 15%)")]
    [Range(0f, 1f)]
    public float characterFollowWeight = 0.15f;

    private Vector3 initialAimPos1;
    private Vector3 initialAimPos2;
    private Vector3 initialCharPos1;
    private Vector3 initialCharPos2;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab; 

    void Start()
    {
        if (aimTarget1 != null) initialAimPos1 = aimTarget1.localPosition;
        if (aimTarget2 != null) initialAimPos2 = aimTarget2.localPosition;
        
        if (characterModel1 != null) initialCharPos1 = characterModel1.localPosition;
        if (characterModel2 != null) initialCharPos2 = characterModel2.localPosition;
    }
    public float spawnDepthFromCamera = 1.0f;

    void Update()
    {
        // Cegah pergerakan dan penembakan saat game sedang Freeze (Countdown)
        if (Time.timeScale == 0f) return;

        MoveCrosshair1();
        MoveCrosshair2();

        // --- UPDATE POSISI AIM TARGET & KARAKTER ---
        UpdateAimTargetPosition(aimTarget1, characterModel1, crosshair1, aimMoveSpeed1, aimScale1, initialAimPos1, initialCharPos1);
        UpdateAimTargetPosition(aimTarget2, characterModel2, crosshair2, aimMoveSpeed2, aimScale2, initialAimPos2, initialCharPos2);

        // Mengirimkan data Player1 saat crosshair 1 menembak
        if (Input.GetKeyDown(shootKey1)) ShootFromCrosshair(crosshair1, PlayerIndex.Player1);
        
        // Mengirimkan data Player2 saat crosshair 2 menembak
        if (Input.GetKeyDown(shootKey2)) ShootFromCrosshair(crosshair2, PlayerIndex.Player2);
    }

    void MoveCrosshair1()
    {
        if (crosshair1 == null) return;
        float moveX = 0f; float moveY = 0f;
        if (Input.GetKey(KeyCode.W)) moveY = 1f;
        if (Input.GetKey(KeyCode.S)) moveY = -1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        Vector2 movement = new Vector2(moveX, moveY).normalized * speed1 * Time.deltaTime;
        
        Vector2 newPos = crosshair1.anchoredPosition + movement;
        ClampCrosshairPosition(crosshair1, ref newPos);
        crosshair1.anchoredPosition = newPos;
    }

    void MoveCrosshair2()
    {
        if (crosshair2 == null) return;
        float moveX = 0f; float moveY = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) moveY = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveY = -1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;
        Vector2 movement = new Vector2(moveX, moveY).normalized * speed2 * Time.deltaTime;
        
        Vector2 newPos = crosshair2.anchoredPosition + movement;
        ClampCrosshairPosition(crosshair2, ref newPos);
        crosshair2.anchoredPosition = newPos;
    }

    void UpdateAimTargetPosition(Transform aimTarget, Transform characterModel, RectTransform crosshair, float moveSpeed, float scale, Vector3 initialAimPos, Vector3 initialCharPos)
    {
        if (aimTarget == null)
        {
            Debug.LogWarning("WARNING: Aim Target belum di-assign di Inspector Crosshair Manager!");
            return;
        }
        if (crosshair == null) return;

        // Mengambil offset dari pergerakan UI Crosshair
        float offsetX = crosshair.anchoredPosition.x * scale;
        float offsetY = crosshair.anchoredPosition.y * scale;

        // 1. MENGGERAKKAN AIM TARGET (100% Offset)
        Vector3 targetLocalPos = new Vector3(initialAimPos.x + offsetX, initialAimPos.y + offsetY, initialAimPos.z);
        aimTarget.localPosition = Vector3.Lerp(aimTarget.localPosition, targetLocalPos, Time.deltaTime * moveSpeed);

        // 2. MENGGERAKKAN MODEL KARAKTER (Berdasarkan persentase characterFollowWeight)
        if (characterModel != null)
        {
            float charOffsetX = offsetX * characterFollowWeight;
            float charOffsetY = offsetY * characterFollowWeight;
            
            Vector3 charTargetPos = new Vector3(initialCharPos.x + charOffsetX, initialCharPos.y + charOffsetY, initialCharPos.z);
            characterModel.localPosition = Vector3.Lerp(characterModel.localPosition, charTargetPos, Time.deltaTime * moveSpeed);
        }
    }

    void ClampCrosshairPosition(RectTransform crosshair, ref Vector2 pos)
    {
        RectTransform parent = crosshair.parent as RectTransform;
        if (parent == null) return;

        // Batas setengah dari parent rect
        float halfWidth = parent.rect.width / 2f;
        float halfHeight = parent.rect.height / 2f;

        // Memberikan sedikit margin berdasarkan ukuran crosshair sendiri
        float paddingX = crosshair.rect.width / 2f;
        float paddingY = crosshair.rect.height / 2f;

        // Asumsi anchor crosshair ada di tengah (Center-Center)
        pos.x = Mathf.Clamp(pos.x, -halfWidth + paddingX, halfWidth - paddingX);
        pos.y = Mathf.Clamp(pos.y, -halfHeight + paddingY, halfHeight - paddingY);
    }

    // Fungsi dimodifikasi untuk menerima identitas Player penembak
    void ShootFromCrosshair(RectTransform crosshair, PlayerIndex player)
    {
        if (projectilePrefab == null || crosshair == null) return;

        Animator anim = (player == PlayerIndex.Player1) ? animator1 : animator2;
        if (anim != null) anim.SetTrigger("Attack");

        Vector3 screenPos = crosshair.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f); 
        }

        Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, spawnDepthFromCamera);
        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(screenPosWithDepth);

        GameObject bullet = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        bullet.transform.LookAt(targetPoint);

        // --- INTEGRASI SCORE MANAGER ---
        // 1. Catat ke peluru ini, bahwa peluru ini adalah milik si Player (1 atau 2)
        Projectile projScript = bullet.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.ownerPlayer = player;
        }

        // 2. Laporkan ke ScoreManager bahwa player ini telah melepaskan 1 kali tembakan
        if (HyperSmashScoreManager.Instance != null)
        {
            HyperSmashScoreManager.Instance.RegisterShot(player);
        }
        // -------------------------------
    }
}
