using UnityEngine;

public class ShootingStarProjectile : MonoBehaviour
{
    [Header("Movement Stats")]
    public float speed = 10f;
    public float lifeTime = 4f;
    public float maxTravelDistance = 20f;

    [Header("Visual Correction")]
    public float visualRotationOffset = 90f;

    [Header("Collision Rules")]
    public LayerMask blockingLayers;

    [Header("Visual Effects")]
    public GameObject impactVFX;

    // --- TAMBAHAN AUDIO SECTION ---
    [Header("Audio Feedback")]
    [Tooltip("SFX saat menabrak dinding atau mencapai jarak maksimal")]
    public AudioClip impactSfx;
    [Range(0, 1)] public float impactVolume = 0.7f;
    [SerializeField] private float soundMaxDistance = 15f; // Jarak dengar maksimal

    private Vector3 lockedMoveDirection;
    private Vector3 _startPosition;
    private bool _isDead = false;

    private void Start()
    {
        _startPosition = transform.position;
        lockedMoveDirection = transform.right;
        transform.Rotate(0, 0, visualRotationOffset);
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_isDead) return;

        transform.position += lockedMoveDirection * speed * Time.deltaTime;

        float currentDistance = Vector3.Distance(_startPosition, transform.position);

        if (currentDistance >= maxTravelDistance)
        {
            HitWall();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

        if (IsTouchingLayer(collision.gameObject.layer, blockingLayers))
        {
            HitWall();
        }
    }

    public void HitWall()
    {
        if (_isDead) return;
        _isDead = true;

        // --- LOGIKA AUDIO IMPACT (SPASIAL & MIXER LINKED) ---
        if (impactSfx != null)
        {
            // Buat objek audio sementara agar suara tidak terputus saat projectile hancur
            GameObject sfxObj = new GameObject("TempSFX_StarImpact");
            sfxObj.transform.position = transform.position;
            AudioSource source = sfxObj.AddComponent<AudioSource>();

            source.clip = impactSfx;
            source.volume = impactVolume;

            // Setting Spasial (3D)
            source.spatialBlend = 1f;
            source.minDistance = 2f;
            source.maxDistance = soundMaxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;

            // Hubungkan ke Audio Mixer Group SFX (Wajib!)
            if (GlobalAudioManager.Instance != null && GlobalAudioManager.Instance.mainMixer != null)
            {
                var sfxGroups = GlobalAudioManager.Instance.mainMixer.FindMatchingGroups("SFX");
                if (sfxGroups.Length > 0) source.outputAudioMixerGroup = sfxGroups[0];
            }

            source.Play();
            Destroy(sfxObj, impactSfx.length);
        }

        if (impactVFX != null)
        {
            Instantiate(impactVFX, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private bool IsTouchingLayer(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) > 0;
    }
}