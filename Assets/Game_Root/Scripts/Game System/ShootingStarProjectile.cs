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

    private Vector3 lockedMoveDirection;
    private Vector3 _startPosition;
    private bool _isDead = false; // Flag biar gak panggil Destroy berkali-kali

    private void Start()
    {
        // CATAT POSISI AWAL SEBELUM GERAK
        _startPosition = transform.position;

        // Ambil arah tembakan dari Launcher (sebelum diputar visualnya)
        lockedMoveDirection = transform.right;

        // Putar visualnya doang
        transform.Rotate(0, 0, visualRotationOffset);

        // Failsafe waktu tetap ada
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_isDead) return;

        // Gerak lurus sesuai arah tembakan awal
        transform.position += lockedMoveDirection * speed * Time.deltaTime;

        // HITUNG JARAK ASLI DARI TITIK START
        float currentDistance = Vector3.Distance(_startPosition, transform.position);

        // DEBUG: Nyalain ini kalau mau liat angkanya di Console pas ngetes
        // Debug.Log($"Distance: {currentDistance} / {maxTravelDistance}");

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