using UnityEngine;
using WizardPunk.HyperSmash; // Memanggil namespace dari sistem lama Anda

public class DualCrosshairController : MonoBehaviour
{
    [Header("Crosshair 1 (WASD)")]
    public RectTransform crosshair1;
    public float speed1 = 500f;
    public KeyCode shootKey1 = KeyCode.Space; 

    [Header("Crosshair 2 (Arrow Keys)")]
    public RectTransform crosshair2;
    public float speed2 = 500f;
    public KeyCode shootKey2 = KeyCode.RightControl; 

    [Header("Shooting Settings")]
    public GameObject projectilePrefab; 
    public float spawnDepthFromCamera = 1.0f;

    void Update()
    {
        // Cegah pergerakan dan penembakan saat game sedang Freeze (Countdown)
        if (Time.timeScale == 0f) return;

        MoveCrosshair1();
        MoveCrosshair2();

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
