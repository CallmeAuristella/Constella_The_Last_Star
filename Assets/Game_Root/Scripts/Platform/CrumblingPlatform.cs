using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CrumblePlatform : MonoBehaviour
{
    [Header("Crumble Settings")]
    public float delayBeforeDisappear = 1.2f;
    public float respawnTime = 3f;

    [Header("Visual Effects")]
    public Color warningColor = Color.red;
    public float shakeMagnitude = 0.05f;

    private bool isTriggered = false;
    private Vector3 _originalPos;
    private SpriteRenderer sr;
    private Collider2D col;
    private Color originalColor;
    private Rigidbody2D rb;

    private void Awake()
    {
        _originalPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Pastikan ada Rigidbody tapi JANGAN kasih gravity
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        SetupPhysics();

        if (sr != null) originalColor = sr.color;
    }

    private void SetupPhysics()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true; // Biar player gak licin
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTriggered || !collision.gameObject.CompareTag("Player")) return;

        // Cek apakah player injak dari atas
        if (collision.contacts[0].normal.y < -0.5f)
        {
            StartCoroutine(VanishAndRespawnSequence());
        }
    }

    private IEnumerator VanishAndRespawnSequence()
    {
        isTriggered = true;

        // FASE 1: PERINGATAN (Getar)
        if (sr != null) sr.color = warningColor;

        float elapsed = 0f;
        while (elapsed < delayBeforeDisappear)
        {
            // Shake visual saja, jangan ganggu fisika utama biar gak aneh
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = _originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = _originalPos; // Kembalikan posisi

        // FASE 2: MENGHILANG
        SetPlatformState(false);

        // FASE 3: TUNGGU
        yield return new WaitForSeconds(respawnTime);

        // FASE 4: MUNCUL
        SetPlatformState(true);
        isTriggered = false;
    }

    private void SetPlatformState(bool active)
    {
        if (sr != null)
        {
            sr.enabled = active;
            sr.color = originalColor;
        }
        if (col != null) col.enabled = active;

        // Penting: Matikan simulasi biar gak ada collider hantu
        rb.simulated = active;
    }
}