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
    [Tooltip("Platform akan reset jika posisi Y lebih rendah dari angka ini")]
    public float fallLimitY = -10f;
    [Tooltip("Centang jika ingin platform reset otomatis tanpa nunggu GameManager")]
    public bool autoResetByPosition = true;

    private Rigidbody2D _rb;
    private Vector2 _originalPosition;
    private bool _isTriggered = false;
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
        // Penting: Memastikan sleeping mode tidak mengganggu
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void Update()
    {
        // LOGIKA SOLUSI LAIN: Cek Batas Y
        if (autoResetByPosition && _isTriggered)
        {
            if (transform.position.y < fallLimitY)
            {
                ResetPlatform();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isTriggered || !collision.gameObject.CompareTag("Player")) return;

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
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = _originalPosition + new Vector2(offsetX, offsetY);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _rb.position = _originalPosition;
        _rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    public void ResetPlatform()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        StopAllCoroutines();

        _isTriggered = false;
        SetupPhysics();

        // Paksa balik posisi pakai kedua sistem (Transform & Rigidbody)
        _rb.position = _originalPosition;
        transform.position = _originalPosition;
        transform.rotation = Quaternion.identity;

        Debug.Log($"[FallingPlatform] {gameObject.name} AUTO-RESET SUCCESS!");
    }
}