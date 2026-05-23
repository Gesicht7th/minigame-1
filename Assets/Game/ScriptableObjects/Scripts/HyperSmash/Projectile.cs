using UnityEngine;
using WizardPunk.HyperSmash;

public class Projectile : MonoBehaviour
{
    public float speed = 150f;
    public float lifeTime = 3f;
    public GameObject hitEffect;

    [HideInInspector]
    public PlayerIndex ownerPlayer;

    private Vector3 lastCameraPos;

    // === FUNGSI INISIALISASI ===
    public void Initialize(Vector3 direction, float projSpeed, float projRange, PlayerIndex owner)
    {
        speed = projSpeed;
        ownerPlayer = owner;

        // --- FIX UTAMA: MEMUTAR MODEL PELURU ---
        // Putar peluru agar moncongnya menghadap tepat ke arah tembakan (crosshair)
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        // ---------------------------------------

        lifeTime = projRange / projSpeed;
        Destroy(gameObject, lifeTime);

        if (HyperSmashSoundController.Instance != null)
        {
            if (ownerPlayer == PlayerIndex.Player1)
                HyperSmashSoundController.Instance.PlayP1ShootSound();
            else
                HyperSmashSoundController.Instance.PlayP2ShootSound();
        }
    }

    void Start()
    {
        if (Camera.main != null)
        {
            lastCameraPos = Camera.main.transform.position;
        }
    }

    void Update()
    {
        if (Camera.main != null)
        {
            Vector3 cameraDelta = Camera.main.transform.position - lastCameraPos;
            transform.position += cameraDelta;
            lastCameraPos = Camera.main.transform.position;
        }

        // Karena model peluru sudah diputar menghadap target di Initialize,
        // kita bisa menggerakkannya lurus ke depan (Space.Self) layaknya peluru asli.
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target") || other.CompareTag("Crystal"))
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }

            Crystal crystal = other.GetComponent<Crystal>();

            if (crystal != null && HyperSmashScoreManager.Instance != null)
            {
                HyperSmashScoreManager.Instance.RegisterHit(ownerPlayer);
                HyperSmashScoreManager.Instance.RegisterCrystalKill(ownerPlayer, crystal);
            }

            other.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}