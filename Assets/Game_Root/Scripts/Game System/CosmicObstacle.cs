using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CosmicObstacle : MonoBehaviour
{
    [Header("Respawn Settings")]
    public bool canRespawn = true;
    public float respawnTime = 3f;

    [Header("Visual Effects")]
    public GameObject destroyVFX;

    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 startPosition;
    private bool isDestroyed = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;

        // TEGAS: Pastikan Collider diatur sebagai Trigger agar sesuai dengan fungsi OnTriggerEnter2D
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Cek apakah yang nabrak adalah Player
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[Obstacle] {gameObject.name} kena Player!");

            // 2. Cek Shield (Fungsi Vital lo)
            PlayerShield shield = collision.GetComponent<PlayerShield>();
            if (shield != null && shield.isShieldActive)
            {
                Debug.Log("[Obstacle] Player pake Shield, cuekin.");
                return;
            }

            // 3. Panggil fungsi mati di PlayerRespawn
            // Biarkan PlayerRespawn yang ngurusin Freeze, Audio, dan Fade biar GAK BENTROK.
            PlayerRespawn respawnScript = collision.GetComponent<PlayerRespawn>();
            if (respawnScript != null)
            {
                respawnScript.DieAndRespawn();
            }
            else
            {
                Debug.LogError("[Obstacle] Player gak punya skrip PlayerRespawn, tod!");
            }
        }
    }

    // Fungsi untuk menghancurkan obstacle ini (misal kena tembakan atau dihancurin player)
    public void HandleDestruction()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (destroyVFX != null)
        {
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }

        if (canRespawn)
        {
            StartCoroutine(ObstacleRespawnRoutine());
        }
        else
        {
            // Jangan pake Destroy kalau mau dipake lagi, pake SetActive(false) aja
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ObstacleRespawnRoutine()
    {
        if (sr != null) sr.enabled = false;
        if (col != null) col.isTrigger = false; // Matikan trigger biar gak kena player pas invisible

        yield return new WaitForSeconds(respawnTime);

        transform.position = startPosition;

        if (sr != null) sr.enabled = true;
        if (col != null) col.isTrigger = true; // Nyalain lagi

        isDestroyed = false;
        Debug.Log($"[Obstacle] {gameObject.name} muncul lagi.");
    }
}