using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class LevelInitializer : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Jika dicentang, player akan pindah ke checkpoint terakhir. Jika tidak, tetap di posisi awal scene.")]
    public bool useGlobalCheckpoint = true;

    private void Awake()
    {
        // Set TimeScale ke 1 seawal mungkin biar gak nyangkut pas ganti scene
        Time.timeScale = 1f;
    }

    private IEnumerator Start()
    {
        // KUNCI UTAMA: Kasih jeda 1 frame (null) biar semua object di scene baru 'bangun' dulu
        yield return null;

        InitializeLevel();
    }

    private void InitializeLevel()
    {
        // 1. Cari Player berdasarkan Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            PlayerMovementInput playerScript = playerObj.GetComponent<PlayerMovementInput>();

            // 2. Daftarkan Player ke GameManager SEBELUM gerakin posisi
            if (GameManager.Instance != null)
            {
                GameManager.Instance.player = playerScript;

                // Jika player lari dari posisi awal, paksa balik ke checkpoint atau posisi start manual
                if (useGlobalCheckpoint && GameManager.Instance.hasCheckpoint)
                {
                    playerScript.ForceTeleport(GameManager.Instance.lastCheckpointPos);
                }

                GameManager.Instance.StartRun();
            }

            // 3. Setup Cinemachine (LOCK TARGET)
            SetupCamera(playerObj.transform);
        }
        else
        {
            Debug.LogError("[LevelInit] ERROR: Player manual gak ketemu di scene ini! Pastikan Prefab Player ada di Hierarchy dan Tag-nya 'Player'.");
        }
    }

    private void SetupCamera(Transform target)
    {
        // Cari Virtual Camera di scene ini
        var vCam = FindFirstObjectByType<CinemachineCamera>();

        if (vCam != null)
        {
            // Set Target
            vCam.Follow = target;

            // PAKSA CINEMACHINE SNAP: Biar gak ada gerakan 'ngejar' dari koordinat 0,0,0
            vCam.OnTargetObjectWarped(target, target.position - vCam.transform.position);
            vCam.ForceCameraPosition(target.position, Quaternion.identity);

            Debug.Log("[LevelInit] Kamera berhasil dikunci ke: " + target.name);
        }
        else
        {
            Debug.LogWarning("[LevelInit] Waduh, CinemachineCamera gak ada di scene ini!");
        }
    }
}