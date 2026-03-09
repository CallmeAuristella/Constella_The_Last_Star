using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SpinningPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float speed = 300f;
    [Tooltip("Centang untuk putaran searah jarum jam (Clockwise).")]
    public bool clockwise = true;

    [Header("Debug Visuals")]
    public float gizmoRadius = 1.5f;
    public Color gizmoColor = Color.yellow;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;

        // --- ANTI-SNAGGING CORE SETTINGS ---
        // Interpolate wajib agar player tidak jitter/gemetar saat berada di atas platform berputar
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Continuous agar collision tidak tembus saat speed tinggi
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Penting: Gunakan standar kontak Kinematic agar tidak 'lengket' berlebihan
        rb.useFullKinematicContacts = false;
    }

    private void FixedUpdate()
    {
        // Hitung arah: Clockwise (-1), Counter-Clockwise (1)
        float direction = clockwise ? -1f : 1f;
        float rotateAmount = speed * Time.fixedDeltaTime * direction;

        // ANTI-SNAGGING: Gunakan MoveRotation untuk kalkulasi rotasi fisik yang presisi
        // Ini memastikan Player 'terseret' ikut berputar tanpa tergelincir aneh
        rb.MoveRotation(rb.rotation + rotateAmount);
    }

    // --- GIZMOS (TIDAK BERUBAH) ---
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

#if UNITY_EDITOR
        Handles.color = gizmoColor;
        Handles.DrawWireDisc(transform.position, Vector3.forward, gizmoRadius);
        float arrowAngle = clockwise ? -45f : 45f;
        Handles.DrawSolidArc(transform.position, Vector3.forward, Vector3.up, arrowAngle, gizmoRadius * 0.2f);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;
        Handles.Label(transform.position + Vector3.up * (gizmoRadius + 0.3f), $"{(clockwise ? "CW" : "CCW")}\n{speed}°/s", style);
#endif
    }
}