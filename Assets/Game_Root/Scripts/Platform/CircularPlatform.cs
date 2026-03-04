using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class CircularPlatform : MonoBehaviour
{
    [Header("Orbit Configuration")]
    public Transform pivotPoint;
    public float speed = 1.0f; // Kecepatan sudut (Radians per second)
    [Tooltip("Centang untuk putaran searah jarum jam.")]
    public bool clockwise = true;

    [Header("Debug Visuals")]
    public Color gizmoColor = Color.cyan;

    // Internal Variables
    private Rigidbody2D rb;
    private float currentAngle;
    private float orbitRadius;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Wajib Kinematic
        rb.useFullKinematicContacts = true; // Agar fisik lebih akurat
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Gerakan mulus
    }

    private void Start()
    {
        // Guard Clause
        if (pivotPoint == null)
        {
            Debug.LogError($"Pivot Point missing on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }

        // Hitung jarak (radius) dan sudut awal relatif terhadap pivot
        Vector3 offset = transform.position - pivotPoint.position;
        orbitRadius = offset.magnitude;
        currentAngle = Mathf.Atan2(offset.y, offset.x);
    }

    private void FixedUpdate()
    {
        if (pivotPoint == null) return;

        MoveInOrbit();
    }

    private void MoveInOrbit()
    {
        // 1. Hitung Sudut Baru
        float direction = clockwise ? -1f : 1f;
        currentAngle += speed * Time.fixedDeltaTime * direction;

        // 2. Hitung Posisi Baru (Trigonometri)
        float x = pivotPoint.position.x + Mathf.Cos(currentAngle) * orbitRadius;
        float y = pivotPoint.position.y + Mathf.Sin(currentAngle) * orbitRadius;
        Vector2 newPos = new Vector2(x, y);

        // 3. Gerakkan menggunakan Fisika (PENTING untuk Player)
        rb.MovePosition(newPos);
    }

    // --- LOGIKA STICKY (Agar Player menempel) ---
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

    // --- VISUALISASI EDITOR ---
    private void OnDrawGizmos()
    {
        if (pivotPoint != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(pivotPoint.position, 0.3f); // Titik Pivot
            Gizmos.DrawLine(pivotPoint.position, transform.position); // Lengan Orbit

#if UNITY_EDITOR
            // Gunakan Handles untuk visualisasi lingkaran yang lebih cantik
            Handles.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);

            // Hitung radius real-time untuk editor
            float currentRad = Vector3.Distance(transform.position, pivotPoint.position);
            Handles.DrawWireDisc(pivotPoint.position, Vector3.forward, currentRad);

            // Panah Arah
            Vector3 directionVec;
            Vector3 toPlayer = (transform.position - pivotPoint.position).normalized;

            if (clockwise)
                directionVec = Vector3.Cross(Vector3.forward, toPlayer); // Tangent CW
            else
                directionVec = Vector3.Cross(toPlayer, Vector3.forward); // Tangent CCW

            Handles.color = gizmoColor;
            Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(directionVec), 1.5f, EventType.Repaint);

            // Label
            GUIStyle style = new GUIStyle();
            style.normal.textColor = gizmoColor;
            style.alignment = TextAnchor.MiddleCenter;
            string dirText = clockwise ? "CW" : "CCW";
            Handles.Label(pivotPoint.position + Vector3.up * 0.5f, $"{dirText} ({speed} rad/s)", style);
#endif
        }
    }
}