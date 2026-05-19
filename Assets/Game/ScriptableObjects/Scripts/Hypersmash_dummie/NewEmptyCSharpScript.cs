using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Pengaturan Kecepatan")]
    [Tooltip("Kecepatan pergerakan kamera")]
    public float speed = 5f;

    [Header("Arah Pergerakan")]
    public Vector3 direction = Vector3.forward; // Default: Maju ke depan

    void Update()
    {
        // Menggerakkan kamera secara terus-menerus setiap frame
        transform.Translate(direction * speed * Time.deltaTime);
    }
}