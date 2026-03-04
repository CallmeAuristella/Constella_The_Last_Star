using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CrumblePlatform : MonoBehaviour
{
    [Header("Crumble Settings")]
    [Tooltip("Waktu platform bertahan sejak diinjak sebelum menghilang")]
    public float delayBeforeDisappear = 1.2f; // Sudah diperlama defaultnya

    [Tooltip("Waktu yang dibutuhkan platform untuk muncul kembali setelah hilang")]
    public float respawnTime = 3f; // [FITUR BARU] Timer Respawn

    [Header("Visual Effects")]
    public Color warningColor = Color.red;
    public float shakeMagnitude = 0.05f;

    private bool isTriggered = false;
    private Vector3 initialPosition;

    // Referensi komponen untuk dimatikan/dinyalakan
    private SpriteRenderer sr;
    private Collider2D col;
    private Color originalColor;

    private void Awake()
    {
        initialPosition = transform.position;

        // AUTO-SETUP
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kalau sedang proses hancur atau hilang, abaikan
        if (isTriggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            ContactPoint2D contact = collision.contacts[0];

            // --- [LOGIKA ANTI KEJEDUG] ---
            bool isHittingFromTop = contact.point.y > transform.position.y;
            bool isPlayerFalling = playerRb != null && playerRb.linearVelocity.y <= 0.1f;

            if (isHittingFromTop && isPlayerFalling)
            {
                StartCoroutine(VanishAndRespawnSequence());
            }
        }
    }

    private IEnumerator VanishAndRespawnSequence()
    {
        isTriggered = true;

        // FASE 1: PERINGATAN (Berubah warna & Getar)
        if (sr != null) sr.color = warningColor;

        float elapsed = 0f;
        while (elapsed < delayBeforeDisappear)
        {
            if (shakeMagnitude > 0)
            {
                transform.position = initialPosition + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition; // Kembalikan posisi setelah getar selesai

        // FASE 2: MENGHILANG (Matikan Fisika dan Visual, JANGAN DI-DESTROY)
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        // FASE 3: TUNGGU RESPAWN
        yield return new WaitForSeconds(respawnTime);

        // FASE 4: MUNCUL KEMBALI (Nyalakan semua fungsi seperti semula)
        if (sr != null)
        {
            sr.color = originalColor;
            sr.enabled = true;
        }
        if (col != null) col.enabled = true;

        // Reset status agar platform bisa diinjak dan hancur lagi
        isTriggered = false;
    }
}