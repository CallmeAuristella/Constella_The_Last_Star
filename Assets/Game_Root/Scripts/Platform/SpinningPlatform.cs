using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
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
        // Pastikan settingan fisik benar untuk platform bergerak
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true; // Opsional: Agar gesekan dengan player lebih presisi
    }

    private void FixedUpdate()
    {
        // Hitung arah: Clockwise (-1), Counter-Clockwise (1)
        float direction = clockwise ? -1f : 1f;
        float rotateAmount = speed * Time.fixedDeltaTime * direction;

        // Rotasi Fisika
        rb.MoveRotation(rb.rotation + rotateAmount);
    }

    private void OnDrawGizmos()
    {
        // Visualisasi Pivot (Selalu muncul)
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

#if UNITY_EDITOR
        // Visualisasi Guide (Hanya di Unity Editor)
        Handles.color = gizmoColor;

        // 1. Lingkaran Radius
        Handles.DrawWireDisc(transform.position, Vector3.forward, gizmoRadius);

        // 2. Indikator Arah (Arc Kecil)
        float arrowAngle = clockwise ? -45f : 45f;
        Handles.DrawSolidArc(transform.position, Vector3.forward, Vector3.up, arrowAngle, gizmoRadius * 0.2f);

        // 3. Label Info
        string dirText = clockwise ? "CW (Kanan)" : "CCW (Kiri)";
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        Handles.Label(transform.position + Vector3.up * (gizmoRadius + 0.3f), $"{dirText}\n{speed}°/s", style);
#endif
    }
}