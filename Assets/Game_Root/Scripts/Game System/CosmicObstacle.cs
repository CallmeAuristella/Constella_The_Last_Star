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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerShield shield = collision.GetComponent<PlayerShield>();

            if (shield != null && shield.isShieldActive) return;

            // --- TAMBAHKAN LOGIKA FREEZE DI SINI ---
            var rb = collision.GetComponent<Rigidbody2D>();
            var input = collision.GetComponent<PlayerMovementInput>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Hentikan gerak seketika
                rb.simulated = false; // Matikan fisika agar tidak meluncur terus
            }

            if (input != null) input.enabled = false; // Matikan kontrol player

            // Baru panggil fungsi respawn bawaan lo
            PlayerRespawn respawnScript = collision.GetComponent<PlayerRespawn>();
            if (respawnScript != null)
            {
                respawnScript.DieAndRespawn();
            }
        }
    }

    public void HandleDestruction()
    {
        // Tambahin pengecekan ini biar variabelnya kepake
        if (isDestroyed) return;

        isDestroyed = true;

        if (destroyVFX != null)
        {
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }

        if (canRespawn)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        transform.position = startPosition;

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;

        isDestroyed = false;
    }
}