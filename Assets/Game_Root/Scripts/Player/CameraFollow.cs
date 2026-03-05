using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private void Start()
    {
        // Cari target sekali di awal start
        FindTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Jangan cari tiap frame! Coba cari lagi lewat fungsi efisien
            FindTarget();
            return;
        }

        // Logic Follow Halus
        Vector3 desiredPosition = target.position + offset;

        // Menggunakan Lerp untuk smoothing
        // Tips: Gunakan Time.deltaTime agar smooth-nya konsisten di semua FPS
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * (Time.deltaTime * 10f));
    }

    private void FindTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            target = playerObj.transform;

            // OPTIONAL: Biar kamera gak nge-lerp (langsung ke posisi player) pas baru nemu
            SnapToTarget();
        }
    }

    // Fungsi untuk memaksa kamera langsung ke posisi target (Tanpa Delay)
    // Panggil ini dari GameManager setelah player teleport/pindah scene
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}