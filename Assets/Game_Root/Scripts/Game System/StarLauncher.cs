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
    public float maxLaserDistance = 50f;
    public LayerMask obstacleLayer;
    public float groundOffset = 0.01f;

    [Header("Timing Settings")]
    public float fireRate = 2f;
    public float startDelay = 0f;

    [Header("Feedback Effects")]
    public GameObject muzzleFlashVFX;

    // --- TAMBAHAN AUDIO SECTION ---
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource launcherAudioSource; // Gunakan AudioSource yang nempel
    [SerializeField] private AudioClip warningSfx;           // Suara laser penanda
    [SerializeField] private AudioClip fireSfx;              // Suara tembakan bintang
    [SerializeField] private float soundMaxDistance = 20f;   // Jarak dengar maksimal

    private float timer;

    private void Start()
    {
        // Setup AudioSource Otomatis jika lupa ditarik
        if (launcherAudioSource == null) launcherAudioSource = GetComponent<AudioSource>();
        SetupAudioSource3D();

        if (projectilePrefab == null)
        {
            Debug.LogError($"[StarLauncher] Projectile Prefab kosong di {gameObject.name}!");
            enabled = false;
            return;
        }

        if (firePoint == null) firePoint = transform;
        timer = fireRate - startDelay;
    }

    private void SetupAudioSource3D()
    {
        if (launcherAudioSource == null) return;

        launcherAudioSource.spatialBlend = 1f; // Full 3D
        launcherAudioSource.playOnAwake = false;
        launcherAudioSource.maxDistance = soundMaxDistance;
        launcherAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Hubungkan ke Mixer SFX
        if (GlobalAudioManager.Instance != null && GlobalAudioManager.Instance.mainMixer != null)
        {
            var sfxGroups = GlobalAudioManager.Instance.mainMixer.FindMatchingGroups("SFX");
            if (sfxGroups.Length > 0) launcherAudioSource.outputAudioMixerGroup = sfxGroups[0];
        }
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

        // PLAY WARNING SFX
        if (warningSfx != null && launcherAudioSource != null)
        {
            launcherAudioSource.PlayOneShot(warningSfx);
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
        // PLAY FIRE SFX
        if (fireSfx != null && launcherAudioSource != null)
        {
            launcherAudioSource.PlayOneShot(fireSfx);
        }

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        ShootingStarProjectile projScript = projObj.GetComponent<ShootingStarProjectile>();

        if (projScript != null)
        {
            projScript.maxTravelDistance = maxLaserDistance;
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