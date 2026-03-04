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
        if (isDestroyed) return;

        // 1. DITABRAK SHIELD
        if (collision.gameObject.CompareTag("Shield"))
        {
            HandleDestruction();
        }
        // 2. DITABRAK PLAYER
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Panggil fungsi mati dari skrip teleport di Player
            PlayerRespawn playerRespawn = collision.gameObject.GetComponent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                playerRespawn.DieAndRespawn();
            }
            else
            {
                Debug.LogError("[CosmicObstacle] Karakter lo belum dipasangi skrip PlayerRespawn!");
            }
        }
    }

    public void HandleDestruction()
    {
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