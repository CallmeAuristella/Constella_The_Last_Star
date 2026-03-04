using UnityEngine;
using System.Collections;

public class StarLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Danger Warning (Ground Target)")]
    public GameObject dangerVisualPrefab;
    public float warningDuration = 0.5f;
    public float maxLaserDistance = 50f; // NILAI INI YANG AKAN DIKIRIM KE PELURU
    public LayerMask obstacleLayer;
    public float groundOffset = 0.01f;

    [Header("Timing Settings")]
    public float fireRate = 2f;
    public float startDelay = 0f;

    [Header("Feedback Effects")]
    public GameObject muzzleFlashVFX;

    private float timer;

    private void Start()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"[StarLauncher] Projectile Prefab kosong di {gameObject.name}!");
            enabled = false;
            return;
        }

        if (firePoint == null) firePoint = transform;
        timer = fireRate - startDelay;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            timer = 0f;
            StartCoroutine(ShootSequenceRoutine());
        }
    }

    private IEnumerator ShootSequenceRoutine()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, maxLaserDistance, obstacleLayer);

        Vector3 warningSpawnPosition = firePoint.position + (firePoint.right * maxLaserDistance);
        Quaternion warningRotation = Quaternion.identity;

        if (hit.collider != null)
        {
            warningSpawnPosition = hit.point + (hit.normal * groundOffset);
            warningRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        if (dangerVisualPrefab != null)
        {
            GameObject warningObj = Instantiate(dangerVisualPrefab, warningSpawnPosition, warningRotation);
            DangerVisual visualScript = warningObj.GetComponent<DangerVisual>();
            if (visualScript != null) visualScript.lifetime = warningDuration;
            else Destroy(warningObj, warningDuration);
        }

        yield return new WaitForSeconds(warningDuration);
        Shoot();
    }

    private void Shoot()
    {
        // 1. Spawn Peluru
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. SINKRONISASI: Ambil script projectile-nya
        ShootingStarProjectile projScript = projObj.GetComponent<ShootingStarProjectile>();

        if (projScript != null)
        {
            // SUNTIK JARAK DARI LAUNCHER KE PELURU
            projScript.maxTravelDistance = maxLaserDistance;

            // Debug buat mastiin angkanya masuk
            // Debug.Log($"Peluru ditembak dengan jarak: {projScript.maxTravelDistance}");
        }

        if (muzzleFlashVFX != null)
            Instantiate(muzzleFlashVFX, firePoint.position, firePoint.rotation);
    }

    private void OnDrawGizmos()
    {
        Transform point = firePoint != null ? firePoint : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point.position, 0.2f);
        Gizmos.DrawRay(point.position, point.right * maxLaserDistance);
    }
}