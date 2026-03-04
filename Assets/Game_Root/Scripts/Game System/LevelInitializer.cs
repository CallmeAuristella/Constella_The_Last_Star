using UnityEngine;
using Unity.Cinemachine; // Pastikan Unity 6 (Cinemachine 3.0+)

public class LevelInitializer : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Drag object SpawnPoint dari Hierarchy ke sini")]
    public Transform playerStartPoint;

    private void Start()
    {
        InitializePlayerAndCamera();
        InitializeGameManager();
    }

    private void InitializePlayerAndCamera()
    {
        // 1. Cari Player berdasarkan Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && playerStartPoint != null)
        {
            // Pindahkan posisi Player ke titik Start
            player.transform.position = playerStartPoint.position;

            // Reset Fisika (Biar gak ada sisa gaya dorong/mental)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero; // Unity 6 pakai linearVelocity

            // Setup Cinemachine (Auto Follow)
            var vCam = FindFirstObjectByType<CinemachineCamera>();
            if (vCam != null)
            {
                vCam.Follow = player.transform;
            }
            else
            {
                Debug.LogWarning("[LevelInit] CinemachineCamera tidak ditemukan di scene ini.");
            }
        }
        else
        {
            Debug.LogError("[LevelInit] Error: Player tidak ditemukan atau SpawnPoint belum diisi!");
        }
    }

    private void InitializeGameManager()
    {
        // Pastikan GameManager ada (dari DontDestroyOnLoad)
        if (GameManager.Instance == null) return;

        // 1. Simpan Checkpoint (Buat respawn kalau mati)
        if (playerStartPoint != null)
        {
            GameManager.Instance.SetCheckpoint(playerStartPoint.position);
        }

        // 2. Mulai Timer & Pastikan Game Jalan (Unpause)
        // Ini fungsi krusial biar waktu lanjut jalan pas pindah scene
        GameManager.Instance.StartRun();
        Time.timeScale = 1f;

        // Note: Setup TransitionScreen dihapus dari sini karena sudah
        // dihandle otomatis oleh script 'SceneFader' di object TransitionScreen.
    }
}