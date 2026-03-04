using UnityEngine;
using UnityEngine.SceneManagement;

public class DevCheats : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag Player dari Hierarchy kesini")]
    public Transform playerTransform;

    [Tooltip("Drag Node/Bintang tujuan teleport (Urut dari 1, 2, 3...)")]
    public Transform[] teleportSpots;

    private void Update()
    {
        // --- FITUR 1: TELEPORT PAKE ANGKA (1-5) ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) TeleportToIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TeleportToIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TeleportToIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TeleportToIndex(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TeleportToIndex(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) TeleportToIndex(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) TeleportToIndex(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) TeleportToIndex(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) TeleportToIndex(8);

        // --- FITUR 2: TELEPORT KE POSISI MOUSE (KLIK KANAN) ---
        if (Input.GetMouseButtonDown(1)) // 0=Kiri, 1=Kanan, 2=Tengah
        {
            TeleportToMouse();
        }

        // --- FITUR 3: INSTANT RELOAD (R) ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void TeleportToIndex(int index)
    {
        if (teleportSpots.Length > index && teleportSpots[index] != null)
        {
            MovePlayer(teleportSpots[index].position);
            Debug.Log($"[DEV] Teleport ke Node {index + 1}");
        }
        else
        {
            Debug.LogWarning("[DEV] Slot Teleport kosong atau belum diisi di Inspector!");
        }
    }

    void TeleportToMouse()
    {
        if (playerTransform == null) return;

        // Ambil posisi mouse di dunia game
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Pastikan Z selalu 0 (karena 2D)

        MovePlayer(mouseWorldPos);
        Debug.Log("[DEV] Teleport to Mouse Click!");
    }

    void MovePlayer(Vector3 targetPos)
    {
        if (playerTransform == null)
        {
            // Coba cari otomatis kalo lupa drag
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else return;
        }

        // 1. Matikan momentum player biar gak mental pas nyampe
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Pindahkan Posisi
        playerTransform.position = targetPos;

        // 3. Update Checkpoint di GameManager (biar kalo mati respawn disitu)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(targetPos);
        }
    }
}