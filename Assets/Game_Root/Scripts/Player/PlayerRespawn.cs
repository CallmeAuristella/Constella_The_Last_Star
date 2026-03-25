using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Setup")]
    public Transform levelStartPoint;
    public float fadeDuration = 0.5f;

    [Header("Audio SFX & BGM")]
    public AudioClip deathSfx;
    private AudioSource audioSource; // Source SFX Player
    private AudioSource bgmSource;   // Source BGM Global

    [HideInInspector]
    public Vector3 currentCheckpoint;

    private Rigidbody2D rb;
    private SpriteRenderer[] allSprites;
    private Collider2D[] allColliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        allSprites = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();

        if (levelStartPoint != null)
        {
            currentCheckpoint = levelStartPoint.position;
            transform.position = currentCheckpoint;
        }
    }

    private void Start()
    {
        // CARA TEGAS: Cari BGM Manager di scene secara otomatis
        // Asumsinya BGM lo ada di objek bernama "BGM_Manager" atau punya komponen AudioSource
        GameObject bgmObj = GameObject.Find("BGM_Manager");
        if (bgmObj != null) bgmSource = bgmObj.GetComponent<AudioSource>();
    }

    public void DieAndRespawn()
    {
        if (!rb.simulated) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerDeathCount++;
            GameManager.Instance.AddDeath();
            Debug.Log("Death Count: " + GameManager.Instance.playerDeathCount);
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // 1. LUMPUNKAN PLAYER & PAUSE BGM
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        var input = GetComponent<PlayerMovementInput>();
        if (input != null) input.enabled = false;

        // PAUSE BGM DISINI
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
            Debug.Log("[Audio] BGM Paused.");
        }

        // 2. MAINKAN SFX MATI
        if (deathSfx != null && audioSource != null)
        {
            audioSource.clip = deathSfx;
            audioSource.Play();
        }

        // 3. FADE IN (Layar Gelap)
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // 4. SEMBUNYIKAN PLAYER
        foreach (var s in allSprites) s.enabled = false;
        foreach (var c in allColliders) c.enabled = false;

        // 5. TUNGGU SFX SELESAI
        if (audioSource != null && audioSource.clip != null)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        // 6. TELEPORT (Layar masih hitam)
        if (input != null) input.ForceTeleport(currentCheckpoint);
        else transform.position = currentCheckpoint;

        // 7. FADE OUT (Layar Terang)
        yield return StartCoroutine(Fade(0f, fadeDuration));

        // 8. BANGKITKAN PLAYER & RESUME BGM
        foreach (var s in allSprites)
        {
            if (s.gameObject.name.Contains("Shield")) continue;
            s.enabled = true;
        }
        foreach (var c in allColliders) c.enabled = true;

        rb.simulated = true;
        if (input != null) input.enabled = true;

        // RESUME BGM DISINI
        if (bgmSource != null)
        {
            bgmSource.UnPause(); // Pake UnPause biar lanjut dari detik terakhir
            Debug.Log("[Audio] BGM Resumed.");
        }

        Debug.Log("[Respawn] Sinkronisasi BGM & Player Berhasil!");
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (GameManager.Instance == null || GameManager.Instance.transitionScreen == null) yield break;
        CanvasGroup group = GameManager.Instance.transitionScreen;
        float startAlpha = group.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
        group.blocksRaycasts = (targetAlpha > 0);
    }
}