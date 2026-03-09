using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementInput : MonoBehaviour
{
    [Header("Input System References")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float jumpForce = 22f;

    [Header("Bouncy Ground Settings")]
    public float bouncyJumpMultiplier = 1.8f;
    public string bouncyGroundTag = "BouncyGround";

    [Header("Physics Feel (Momentum)")]
    public float groundAcceleration = 90f;
    public float groundDeceleration = 90f;
    public float airAcceleration = 15f;
    public float airTurnSpeed = 50f;
    public float airDrag = 0f;

    [Header("Gravity Modifiers")]
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 2.5f;

    [Header("Slope Settings")]
    public float slopeCheckDistance = 0.5f;
    public float maxSlopeAngle = 45f;
    public float uphillSpeedMultiplier = 0.8f;
    public float downhillSpeedMultiplier = 1.2f;

    [Header("Jump Rules")]
    public int maxAirJumps = 2;
    public float jumpCooldown = 0.2f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.1f;
    public float groundedGraceTime = 0.1f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Orbit System")]
    public float orbitLaunchMultiplier = 1.5f;
    public bool isOrbiting = false;

    [Header("Visual Effects")]
    public GameObject jumpDust;
    public GameObject landDust;
    public Transform feetPosition;

    // --- Internal Variables ---
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private int currentAirJumpCount = 0;
    private float lastJumpTime = -10f;
    private float groundedGraceCounter;
    private float coyoteTimeCounter;

    // --- Orbit & Platform Variables ---
    private Transform currentOrbitCenter;
    private float currentOrbitSpeed;
    private float currentOrbitRadius;
    private bool clockwiseOrbit;
    private bool isMajorOrbit = false;
    private float totalOrbitAngle = 0f;

    private Transform currentGroundTransform;
    private Rigidbody2D currentGroundRb;
    private Vector3 lastGroundPosition;
    private Vector2 calculatedPlatformVelocity;
    private float platformVelocityAtJump;
    private bool isOnBouncyGround;

    // --- Slope Variables ---
    private Vector2 slopeNormalPerp;
    private bool isOnSlope;
    private float slopeDownAngle;

    // --- Audio Variables ---
    public AudioClip jumpSfx;
    private AudioSource audioSource;

    private void Awake()
    {
       rb = GetComponent<Rigidbody2D>();
        // SETUP AUDIO OTOMATIS
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    } 

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.hasCheckpoint)
        {
            transform.position = GameManager.Instance.lastCheckpointPos;
            rb.linearVelocity = Vector2.zero;
        }

    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += DoJump;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null)
        {
            jumpAction.action.performed -= DoJump;
            jumpAction.action.Disable();
        }
    }

    private void Update()
    {
        if (isOrbiting) { HandleOrbitMovement(); return; }

        moveInput = moveAction.action.ReadValue<Vector2>();
        CheckGroundAndReset();
        SlopeCheck();
        HandleCoyoteTime();
        HandleGravityModifiers();
    }

    private void FixedUpdate()
    {
        if (isOrbiting) return;
        UpdatePlatformRadar();
        HandleMomentumMovement();

        if (rb.linearVelocity.magnitude > 50f)
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 50f);
    }

    // --- COLLISION LOGIC (PARENTING) ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Deteksi platform bergerak agar player menempel dan tidak mantul
        if (collision.gameObject.CompareTag("Platform") || collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Lepas parent saat meninggalkan platform
        if (transform.parent == collision.transform)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
        }
    }

    // --- MOVEMENT & RADAR LOGIC ---

    private void HandleMomentumMovement()
    {
        float inputX = moveInput.x;

        if (isGrounded && isOnSlope && Mathf.Abs(inputX) > 0.01f)
        {
            float targetSlopeSpeed = moveSpeed;
            if (inputX * slopeNormalPerp.x < 0) targetSlopeSpeed *= uphillSpeedMultiplier;
            else targetSlopeSpeed *= downhillSpeedMultiplier;

            rb.linearVelocity = new Vector2(-inputX * targetSlopeSpeed * slopeNormalPerp.x, -inputX * targetSlopeSpeed * slopeNormalPerp.y);
        }
        else
        {
            float currentSpeed = rb.linearVelocity.x;
            float targetSpeed = inputX * moveSpeed;
            float accelRate = isGrounded ? ((Mathf.Abs(inputX) > 0.01f) ? groundAcceleration * 0.65f : groundDeceleration * 0.4f) :
                                           ((Mathf.Abs(inputX) < 0.01f) ? 0f : (Mathf.Sign(inputX) != Mathf.Sign(currentSpeed)) ? airTurnSpeed * 0.55f : airAcceleration);

            float movement = (targetSpeed - currentSpeed) * accelRate * Time.fixedDeltaTime;
            float finalVelocityY = rb.linearVelocity.y;

            // Stabilisasi Velocity di atas platform bergerak
            if ((isGrounded || groundedGraceCounter > 0f) && Time.time > lastJumpTime + 0.2f)
            {
                if (finalVelocityY > calculatedPlatformVelocity.y) finalVelocityY = calculatedPlatformVelocity.y;
                if (calculatedPlatformVelocity.y <= 0.01f && finalVelocityY > -2f) finalVelocityY = -2f;
            }

            rb.linearVelocity = new Vector2(currentSpeed + movement, finalVelocityY);
        }
    }

    private void CheckGroundAndReset()
    {
        bool wasGrounded = isGrounded;
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = groundCollider != null;

        if (isGrounded)
        {
            Transform groundTransform = groundCollider.transform;
            isOnBouncyGround = groundCollider.CompareTag(bouncyGroundTag);

            if (currentGroundTransform != groundTransform)
            {
                currentGroundTransform = groundTransform;
                currentGroundRb = groundCollider.attachedRigidbody;
                lastGroundPosition = groundTransform.position;
            }
        }
        else
        {
            currentGroundTransform = null;
            currentGroundRb = null;
            isOnBouncyGround = false;
        }

        if (!wasGrounded && isGrounded)
        {
            currentAirJumpCount = 0;
            platformVelocityAtJump = 0f;
            if (landDust != null && feetPosition != null) Instantiate(landDust, feetPosition.position, Quaternion.identity);
        }

        if (isGrounded && rb.linearVelocity.y <= 0.1f) coyoteTimeCounter = coyoteTime;
        if (isGrounded) groundedGraceCounter = groundedGraceTime;
        else groundedGraceCounter -= Time.deltaTime;
    }

    private void UpdatePlatformRadar()
    {
        if (isGrounded && currentGroundTransform != null)
        {
            if (currentGroundRb != null)
            {
                Vector2 contactPoint = feetPosition != null ? (Vector2)feetPosition.position : (Vector2)transform.position;
                calculatedPlatformVelocity = currentGroundRb.GetPointVelocity(contactPoint);
            }
            else
            {
                calculatedPlatformVelocity = (currentGroundTransform.position - lastGroundPosition) / Time.fixedDeltaTime;
            }
            lastGroundPosition = currentGroundTransform.position;
        }
        else
        {
            calculatedPlatformVelocity = Vector2.zero;
        }
    }

    private void SlopeCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, slopeCheckDistance, groundLayer);
        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);
            isOnSlope = (slopeDownAngle != 0 && slopeDownAngle <= maxSlopeAngle);
        }
        else isOnSlope = false;
    }

    // --- JUMP LOGIC ---

    private void DoJump(InputAction.CallbackContext context)
    {
        if (Time.time < lastJumpTime + jumpCooldown || isOrbiting) return;

        if (isGrounded || coyoteTimeCounter > 0f)
        {
            PerformJumpPhysics();
            coyoteTimeCounter = 0f;
        }
        else if (currentAirJumpCount < maxAirJumps)
        {
            PerformJumpPhysics();
            currentAirJumpCount++;
        }
    }

    private void PerformJumpPhysics()
    {
        // Lepas parent sebelum lompat agar tidak ada tarikan physics dari platform
        if (transform.parent != null)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
        }

        float platformBoostY = 0f;
        if (isGrounded || coyoteTimeCounter > 0f)
        {
            platformBoostY = calculatedPlatformVelocity.y;
            platformVelocityAtJump = platformBoostY;
        }
        // Audio
        if (jumpSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSfx);
        }
        //
        float finalJumpForce = isOnBouncyGround ? (jumpForce * bouncyJumpMultiplier) : jumpForce;

        // Terapkan kecepatan platform + Jump Force (Anti Lompatan Pendek)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, platformBoostY);
        rb.AddForce(Vector2.up * finalJumpForce, ForceMode2D.Impulse);

        lastJumpTime = Time.time;
        if (jumpDust != null && feetPosition != null) Instantiate(jumpDust, feetPosition.position, Quaternion.identity);


    }

    private void HandleGravityModifiers()
    {
        if (isGrounded || groundedGraceCounter > 0f) return;
        float relativeVelocityY = rb.linearVelocity.y - platformVelocityAtJump;
        if (relativeVelocityY < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        else if (relativeVelocityY > 0 && !jumpAction.action.IsPressed())
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
    }

    private void HandleCoyoteTime() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }

    // --- ORBIT SYSTEM ---

    public void EnterOrbit(Transform centerNode, float radius, float speed, bool isMajor = false)
    {
        isOrbiting = true;
        currentOrbitCenter = centerNode;
        currentOrbitRadius = radius;
        currentOrbitSpeed = speed;
        isMajorOrbit = isMajor;
        totalOrbitAngle = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        clockwiseOrbit = transform.position.x < centerNode.position.x;

        Vector3 direction = (transform.position - centerNode.position).normalized;
        transform.position = centerNode.position + direction * radius;
    }

    private void HandleOrbitMovement()
    {
        if (currentOrbitCenter == null) return;
        if (isMajorOrbit && totalOrbitAngle >= 1080f)
        {
            StarNode node = currentOrbitCenter.GetComponent<StarNode>();
            if (node != null) node.OnOrbitFinished();
            DropFromOrbit();
            return;
        }

        float rotationStep = currentOrbitSpeed * Time.deltaTime;
        float direction = clockwiseOrbit ? -1f : 1f;
        transform.RotateAround(currentOrbitCenter.position, Vector3.forward, rotationStep * direction);
        transform.rotation = Quaternion.identity;
        totalOrbitAngle += rotationStep;

        if (!isMajorOrbit && jumpAction.action.WasPerformedThisFrame()) PerformExitOrbit();
    }

    private void DropFromOrbit()
    {
        isOrbiting = false;
        currentOrbitCenter = null;
        isMajorOrbit = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
    }

    private void PerformExitOrbit()
    {
        Vector2 directionToCenter = (currentOrbitCenter.position - transform.position).normalized;
        Vector2 launchDir = Vector2.Perpendicular(directionToCenter) * (clockwiseOrbit ? -1f : 1f);
        StarNode nodeScript = currentOrbitCenter.GetComponent<StarNode>();
        if (nodeScript != null) nodeScript.IgniteNode();
        ExitOrbit(launchDir, jumpForce * orbitLaunchMultiplier);
    }

    public void ExitOrbit(Vector2 launchDirection, float launchForce)
    {
        isOrbiting = false;
        currentOrbitCenter = null;
        isMajorOrbit = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
    }

    // --- TELEPORT & WARP SYSTEM ---

    public void ForceTeleport(Vector3 newPosition)
    {
        // PERBAIKAN ERROR CINEMACHINE: Lepas parent di baris pertama
        if (transform.parent != null)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
        }

        isOrbiting = false;
        currentOrbitCenter = null;
        isMajorOrbit = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        currentAirJumpCount = 0;

        RigidbodyInterpolation2D oldInterpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation2D.None;

        Vector3 delta = newPosition - transform.position;
        transform.position = newPosition;

        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
        {
            if (brain.ActiveVirtualCamera is CinemachineCamera vcam)
                vcam.OnTargetObjectWarped(transform, delta);
        }
        rb.interpolation = oldInterpolation;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}