using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject meteorPrefab;
    [Tooltip("Prefab visual tanda bahaya di tanah (Target/Garis).")]
    public GameObject dangerVisualPrefab;

    [Header("Spawn Timing")]
    public float spawnInterval = 1.5f;
    public float warningDuration = 1.0f;

    [Header("Area Settings")]
    public float areaWidth = 10f;

    [Header("Ground Targeting (Raycast)")]
    [Tooltip("Jarak maksimal deteksi tanah ke bawah. Nilai ini juga akan menjadi batas hancur Meteor.")]
    public float maxDropDistance = 50f;
    public LayerMask groundLayer;
    public float groundOffset = 0.05f;

    [Header("Variation Settings")]
    public Vector2 sizeVariation = new Vector2(0.8f, 1.2f);

    // --- TAMBAHAN AUDIO SECTION ---
    [Header("Audio SFX (Spatial)")]
    [Tooltip("Suara saat tanda bahaya muncul di tanah")]
    public AudioClip warningSfx;
    [Tooltip("Suara saat meteor muncul di langit")]
    public AudioClip launchSfx;
    [Range(0, 1)] public float sfxVolume = 1f;

    private float timer;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (meteorPrefab == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            StartCoroutine(SpawnSequenceRoutine());
        }
    }

    private IEnumerator SpawnSequenceRoutine()
    {
        float randomX = Random.Range(-areaWidth / 2f, areaWidth / 2f);
        Vector3 skyPos = new Vector3(transform.position.x + randomX, transform.position.y, 0);

        RaycastHit2D hit = Physics2D.Raycast(skyPos, Vector2.down, maxDropDistance, groundLayer);

        if (hit.collider != null)
        {
            Vector3 groundPos = hit.point + (hit.normal * groundOffset);

            // 1. PLAY WARNING SFX (Di posisi tanah)
            if (warningSfx != null)
            {
                // PlayClipAtPoint membuat audio 3D otomatis di koordinat tertentu
                AudioSource.PlayClipAtPoint(warningSfx, groundPos, sfxVolume);
            }

            if (dangerVisualPrefab != null)
            {
                Quaternion warningRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                GameObject warningObj = Instantiate(dangerVisualPrefab, groundPos, warningRot);

                DangerVisual visualScript = warningObj.GetComponent<DangerVisual>();
                if (visualScript != null)
                {
                    visualScript.lifetime = warningDuration;
                }
                else
                {
                    Destroy(warningObj, warningDuration);
                }
            }
        }

        yield return new WaitForSeconds(warningDuration);
        SpawnMeteorAtPosition(skyPos);
    }

    private void SpawnMeteorAtPosition(Vector3 skyPos)
    {
        // 2. PLAY LAUNCH SFX (Di posisi langit saat muncul)
        if (launchSfx != null)
        {
            AudioSource.PlayClipAtPoint(launchSfx, skyPos, sfxVolume);
        }

        Quaternion lockedRotation = Quaternion.Euler(0, 0, 0);
        GameObject newMeteor = Instantiate(meteorPrefab, skyPos, lockedRotation);

        MeteorLogic logic = newMeteor.GetComponent<MeteorLogic>();
        if (logic != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(skyPos, Vector2.down, maxDropDistance, groundLayer);

            if (hit.collider != null)
            {
                logic.SetTargetY(hit.point.y - 0.5f);
            }
            else
            {
                logic.SetTargetY(transform.position.y - maxDropDistance);
            }
        }

        float randomSize = Random.Range(sizeVariation.x, sizeVariation.y);
        newMeteor.transform.localScale = Vector3.one * randomSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(areaWidth, 1f, 0));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaWidth, 1f, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * maxDropDistance);
    }
}