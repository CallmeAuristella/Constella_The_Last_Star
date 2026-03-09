using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LinearPlatform : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement Settings")]
    public float speed = 3.0f;
    public float reachThreshold = 0.05f;

    [Header("Editor Visuals")]
    public Color pathColor = Color.green;

    private Rigidbody2D rb;
    private Vector3 localTarget;
    private Vector3 currentLocalPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- ANTI-SNAGGING SETTINGS ---
        rb.bodyType = RigidbodyType2D.Kinematic;

        // HARUS Interpolate! Agar posisi player dan platform sinkron di setiap frame render.
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Gunakan Continuous agar player tidak 'tembus' saat platform bergerak cepat.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"Points missing on {gameObject.name}!");
            enabled = false;
            return;
        }

        currentLocalPos = pointA.localPosition;
        transform.localPosition = currentLocalPos;
        localTarget = pointB.localPosition;
    }

    private void FixedUpdate()
    {
        // Jika tidak ada parent, gunakan world space biasa
        if (transform.parent == null)
        {
            MoveInWorldSpace();
            return;
        }

        // 1. GERAK SECARA LOKAL
        currentLocalPos = Vector3.MoveTowards(currentLocalPos, localTarget, speed * Time.fixedDeltaTime);

        // 2. SINKRONISASI KE WORLD POSITION MENGGUNAKAN MOVEPOSITION
        // MovePosition adalah kunci agar physics engine tahu objek ini 'membawa' sesuatu (Player)
        Vector3 nextWorldPos = transform.parent.TransformPoint(currentLocalPos);
        rb.MovePosition(nextWorldPos);

        // 3. SWITCH TARGET
        if (Vector3.Distance(currentLocalPos, localTarget) < reachThreshold)
        {
            SwitchLocalTarget();
        }
    }

    private void MoveInWorldSpace()
    {
        // Fallback jika platform tidak punya parent
        Vector3 currentWorldPos = Vector3.MoveTowards(transform.position, transform.parent == null ? localTarget : transform.parent.TransformPoint(localTarget), speed * Time.fixedDeltaTime);
        rb.MovePosition(currentWorldPos);
    }

    private void SwitchLocalTarget()
    {
        localTarget = (localTarget == pointB.localPosition) ? pointA.localPosition : pointB.localPosition;
    }

    // --- LOGIKA STICKY PLAYER (ANTI-SLIP) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Pastikan hanya menempel jika player menginjak dari ATAS
        if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y < -0.5f)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
            // Penting: Pastikan scale player tidak hancur saat lepas parent
            collision.transform.localScale = Vector3.one;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = pathColor;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
    }
}