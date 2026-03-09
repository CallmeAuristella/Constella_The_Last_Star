using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class FallingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public float fallDelay = 0.5f;
    public float shakeMagnitude = 0.05f;
    public float fallSpeed = 2f;

    [Header("Auto Reset Settings")]
    public float fallLimitY = -10f;
    public bool autoResetByPosition = true;

    private Rigidbody2D _rb;
    private Vector2 _originalPosition;
    private bool _isTriggered = false;
    private bool _isFalling = false; // Flag baru untuk kontrol FixedUpdate
    private Coroutine _activeRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalPosition = transform.position;
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // FIX SNAGGING: Pastikan Kinematic berinteraksi halus dengan Dynamic (Player)
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (autoResetByPosition && _isTriggered)
        {
            if (transform.position.y < fallLimitY)
            {
                ResetPlatform();
            }
        }
    }

    // FIX JITTER: Pergerakan Kinematic paling aman di FixedUpdate pake MovePosition
    private void FixedUpdate()
    {
        if (_isFalling)
        {
            Vector2 targetPos = _rb.position + Vector2.down * fallSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPos);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isTriggered || !collision.gameObject.CompareTag("Player")) return;

        // Cek apakah player injak dari atas (normal.y negatif berarti kontak dari bawah kaki player)
        if (collision.contacts[0].normal.y < -0.5f)
        {
            _activeRoutine = StartCoroutine(SinkSequence());
        }
    }

    private IEnumerator SinkSequence()
    {
        _isTriggered = true;

        float elapsedTime = 0f;
        while (elapsedTime < fallDelay)
        {
            // FIX SHAKE: Gunakan offset dari posisi asli tanpa ngerusak sinkronisasi fisik
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            // Pindahkan via Rigidbody biar Player tetep "nempel" stabil di atasnya
            _rb.MovePosition(_originalPosition + new Vector2(offsetX, offsetY));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isFalling = true; // Mulai jatuh di FixedUpdate
    }

    public void ResetPlatform()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        _isTriggered = false;
        _isFalling = false; // Berhenti jatuh

        SetupPhysics();

        _rb.position = _originalPosition;
        transform.position = _originalPosition;
        transform.rotation = Quaternion.identity;

        Debug.Log($"[FallingPlatform] {gameObject.name} AUTO-RESET SUCCESS!");
    }
}