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
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.None; // Kunci biar gak telat 1 frame
    }

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"Points missing on {gameObject.name}!");
            enabled = false;
            return;
        }

        // Set awal di posisi lokal Point A
        currentLocalPos = pointA.localPosition;
        transform.localPosition = currentLocalPos;
        localTarget = pointB.localPosition;
    }

    private void FixedUpdate()
    {
        if (transform.parent == null) return;

        // 1. GERAK SECARA LOKAL (Tidak peduli parent jatuh/terbang)
        currentLocalPos = Vector3.MoveTowards(currentLocalPos, localTarget, speed * Time.fixedDeltaTime);

        // 2. SINKRONISASI KE WORLD POSITION (Agar nempel di parent)
        // Kita paksa posisi dunianya = Posisi Parent + (Offset Lokal)
        Vector3 nextWorldPos = transform.parent.TransformPoint(currentLocalPos);
        rb.position = nextWorldPos;

        // 3. CEK JARAK LOKAL UNTUK SWITCH TARGET
        if (Vector3.Distance(currentLocalPos, localTarget) < reachThreshold)
        {
            SwitchLocalTarget();
        }
    }

    private void SwitchLocalTarget()
    {
        // Tukar target antara posisi lokal A dan B
        localTarget = (localTarget == pointB.localPosition) ? pointA.localPosition : pointB.localPosition;
    }

    // --- LOGIKA STICKY PLAYER ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
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