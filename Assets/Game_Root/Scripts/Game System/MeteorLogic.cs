using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeteorLogic : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float lifeTime = 5f;
    // Kita ganti MaxDistance jadi Target Y Position biar presisi
    public float targetYDestruction;
    private bool useTargetY = false;

    [Header("Collision Rules")]
    public LayerMask destructionLayers;

    [Header("Visual Feedback")]
    public GameObject explosionVFX;

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

        // CEK POSISI: Kalau posisi meteor sudah melewati (kurang dari) Target Y
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

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}