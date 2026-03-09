using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeteorLogic : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float lifeTime = 5f;
    public float targetYDestruction;
    private bool useTargetY = false;

    [Header("Collision Rules")]
    public LayerMask destructionLayers;

    [Header("Visual Feedback")]
    public GameObject explosionVFX;

    // --- TAMBAHAN AUDIO SECTION ---
    [Header("Audio Feedback")]
    [Tooltip("SFX Ledakan saat menyentuh tanah atau player")]
    public AudioClip explosionSfx;
    [Range(0, 1)] public float explosionVolume = 1f;

    private bool hasExploded = false;
    private Rigidbody2D rb;

    public void SetTargetY(float yValue)
    {
        targetYDestruction = yValue;
        useTargetY = true;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (hasExploded) return;

        if (useTargetY && transform.position.y <= targetYDestruction)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other.gameObject);
    }

    private void HandleImpact(GameObject other)
    {
        if (hasExploded) return;

        bool isDestructionLayer = ((1 << other.layer) & destructionLayers) != 0;
        bool isPlayer = other.CompareTag("Player");

        if (isPlayer)
        {
            if (GameManager.Instance != null) GameManager.Instance.RespawnPlayer();
            Explode();
        }
        else if (isDestructionLayer)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // --- LOGIKA AUDIO LEDAKAN (SPASIAL) ---
        if (explosionSfx != null)
        {
            // Kita gunakan GlobalAudioManager untuk memutar SFX agar terikat ke Mixer SFX
            // Jika lo belum buat fungsi PlaySfxAtPoint di Manager, kita pakai cara ini:
            GameObject sfxObj = new GameObject("TempSFX_Explosion");
            sfxObj.transform.position = transform.position;
            AudioSource source = sfxObj.AddComponent<AudioSource>();

            source.clip = explosionSfx;
            source.volume = explosionVolume;
            source.spatialBlend = 1f; // FULL 3D
            source.minDistance = 5f;
            source.maxDistance = 20f;
            source.rolloffMode = AudioRolloffMode.Linear;

            // COLOK KE MIXER (Tegas!)
            if (GlobalAudioManager.Instance != null && GlobalAudioManager.Instance.mainMixer != null)
            {
                source.outputAudioMixerGroup = GlobalAudioManager.Instance.mainMixer.FindMatchingGroups("SFX")[0];
            }

            source.Play();
            Destroy(sfxObj, explosionSfx.length);
        }

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}