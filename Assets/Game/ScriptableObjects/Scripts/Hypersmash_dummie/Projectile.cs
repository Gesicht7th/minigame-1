using UnityEngine;
using WizardPunk.HyperSmash; // Memanggil namespace dari sistem lama Anda

public class Projectile : MonoBehaviour
{
    public float speed = 150f; 
    public float lifeTime = 3f;
    public GameObject hitEffect;
    
    // Variabel baru untuk menyimpan identitas penembak (Player 1 atau Player 2)
    [HideInInspector]
    public PlayerIndex ownerPlayer; 

    private Vector3 lastCameraPos;

    void Start()
    {
        Destroy(gameObject, lifeTime);
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

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target") || other.CompareTag("Crystal"))
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }

            // --- INTEGRASI SCORE MANAGER ---
            // Ambil komponen Crystal dari balok yang tertabrak
            Crystal crystal = other.GetComponent<Crystal>();
            
            if (crystal != null && HyperSmashScoreManager.Instance != null)
            {
                // Laporkan bahwa peluru ini sukses mengenai target
                HyperSmashScoreManager.Instance.RegisterHit(ownerPlayer);
                
                // Tambahkan skor ke player yang memiliki peluru ini
                HyperSmashScoreManager.Instance.RegisterCrystalKill(ownerPlayer, crystal);
            }
            // -------------------------------

            other.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
