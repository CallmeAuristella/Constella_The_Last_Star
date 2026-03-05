using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Setup")]
    public Transform levelStartPoint;
    public float fadeDuration = 0.5f;

    [HideInInspector]
    public Vector3 currentCheckpoint;

    private Rigidbody2D rb;
    private SpriteRenderer[] allSprites;
    private Collider2D[] allColliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        allSprites = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();

        if (levelStartPoint != null)
        {
            currentCheckpoint = levelStartPoint.position;
            transform.position = new Vector3(currentCheckpoint.x, currentCheckpoint.y, transform.position.z);
        }
    }

    public void DieAndRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // 1. Lumpuhkan Player
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;

        // 2. FADE IN (Layar Gelap)
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // 3. Sembunyikan Player
        foreach (var s in allSprites) s.enabled = false;
        foreach (var c in allColliders) c.enabled = false;

        // 4. Teleport
        transform.position = new Vector3(currentCheckpoint.x, currentCheckpoint.y, transform.position.z);

        // 5. FADE OUT (Layar Terang)
        yield return StartCoroutine(Fade(0f, fadeDuration));

        // 6. BANGKITKAN PLAYER (BAGIAN KRUSIAL)
        foreach (var s in allSprites)
        {
            if (s.gameObject.name.Contains("Shield")) continue;
            s.enabled = true;
        }

        foreach (var c in allColliders) c.enabled = true;

        // --- PASTIKAN BARIS INI ADA DI PALING BAWAH ---
        rb.simulated = true; // Nyalain Fisika lagi

        var input = GetComponent<PlayerMovementInput>();
        if (input != null) input.enabled = true; // Nyalain Kontrol lagi

        Debug.Log("[Respawn] Player Unfreezed & Ready!");
    }

    // --- FUNGSI FADE DENGAN CANVASGROUP ---
    private IEnumerator Fade(float targetAlpha, float duration)
    {
        // Ambil CanvasGroup dari GameManager
        CanvasGroup group = GameManager.Instance.transitionScreen;
        if (group == null) yield break;

        group.blocksRaycasts = (targetAlpha > 0); // Kunci layar biar nggak bisa diklik pas gelap
        float startAlpha = group.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
    }
}