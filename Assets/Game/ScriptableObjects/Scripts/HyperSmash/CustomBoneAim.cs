using UnityEngine;

public class CustomBoneAim : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Objek/Transform yang menjadi pusat rotasi (misal: GameObject kosong di dada/bahu)")]
    public Transform sharedPivot;
    
    [Tooltip("Objek yang akan ditarget (misal: objek yang mengikuti posisi Cursor di World Space)")]
    public Transform aimTarget;

    [Tooltip("Bone pertama yang ingin digerakkan")]
    public Transform bone1;
    
    [Tooltip("Bone kedua yang ingin digerakkan (berbeda parent tidak masalah)")]
    public Transform bone2;

    [Header("Settings")]
    [Tooltip("Sumbu dari Pivot yang dianggap sebagai 'depan' (biasanya Z / Vector3.forward)")]
    public Vector3 aimAxis = Vector3.forward;

    // LateUpdate SANGAT PENTING agar tidak merusak/tertimpa animasi dari Animator
    void LateUpdate()
    {
        if (sharedPivot == null || aimTarget == null || bone1 == null || bone2 == null)
            return;

        // 1. Tentukan arah dari pivot menuju target cursor
        Vector3 directionToTarget = aimTarget.position - sharedPivot.position;
        if (directionToTarget.sqrMagnitude < 0.001f) return; // Mencegah error jika terlalu dekat

        // 2. Hitung rotasi yang dibutuhkan agar sumbu depan pivot mengarah ke target
        Vector3 currentForward = sharedPivot.rotation * aimAxis;
        Quaternion targetRotation = Quaternion.FromToRotation(currentForward, directionToTarget) * sharedPivot.rotation;

        // 3. Hitung selisih (delta) antara rotasi target dengan rotasi pivot saat ini
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(sharedPivot.rotation);

        // 4. Aplikasikan selisih rotasi tersebut ke kedua bone mengelilingi pivot
        RotateBoneAroundPivot(bone1, sharedPivot.position, rotationDelta);
        RotateBoneAroundPivot(bone2, sharedPivot.position, rotationDelta);
    }

    /// <summary>
    /// Memutar posisi dan orientasi bone mengelilingi titik pivot.
    /// </summary>
    private void RotateBoneAroundPivot(Transform bone, Vector3 pivotPos, Quaternion rotationDelta)
    {
        // a. Pindahkan posisi bone (orbit mengelilingi pivot)
        Vector3 currentOffset = bone.position - pivotPos;
        bone.position = pivotPos + (rotationDelta * currentOffset);

        // b. Putar orientasi (rotasi) bone itu sendiri
        bone.rotation = rotationDelta * bone.rotation;
    }
}
