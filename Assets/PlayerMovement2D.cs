using UnityEngine;

/// <summary>
/// Mario-style 2D platformer movement controller.
/// Attach to your player GameObject along with a Rigidbody2D and a Collider2D.
/// Requires: Rigidbody2D (set Gravity Scale ~3-4), a ground layer mask.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 60f;      // ground accel
    [SerializeField] private float deceleration = 70f;      // ground friction/braking
    [SerializeField] private float airAcceleration = 40f;   // less control in air
    [SerializeField] private float airDeceleration = 40f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;   // faster fall than rise
    [SerializeField] private float lowJumpGravityMultiplier = 3.5f; // if you release jump early
    [SerializeField] private float maxFallSpeed = 18f;

    [Header("Feel / Forgiveness")]
    [SerializeField] private float coyoteTime = 0.1f;       // grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f;   // grace period before landing

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Squash & Stretch (optional, purely visual)")]
    [SerializeField] private bool useSquashStretch = true;
    [SerializeField] private Transform visualsTransform; // child object holding the sprite

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpHeld;
    private bool jumpPressedThisFrame;

    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool wasGrounded;
    private Vector3 baseScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        if (visualsTransform != null)
            baseScale = visualsTransform.localScale;
    }

    private void Update()
    {
        // --- Input ---
        moveInput = Input.GetAxisRaw("Horizontal"); // -1, 0, 1

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedThisFrame = true;
            jumpBufferTimer = jumpBufferTime;
        }
        jumpHeld = Input.GetButton("Jump");

        // --- Timers ---
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // Facing direction flip (assumes sprite faces right by default)
        if (moveInput != 0 && visualsTransform != null)
        {
            Vector3 scale = visualsTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput);
            visualsTransform.localScale = scale;
        }

        // Landing squash
        if (useSquashStretch && !wasGrounded && isGrounded)
            StopAllCoroutines();

        jumpPressedThisFrame = false; // consumed each frame after buffer check in FixedUpdate ordering below
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleHorizontalMovement();
        HandleJump();
        HandleGravity();

        wasGrounded = isGrounded;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private void HandleHorizontalMovement()
    {
        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate;
        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;

        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        bool canJump = coyoteTimer > 0f;
        bool wantsJump = jumpBufferTimer > 0f;

        if (canJump && wantsJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;

            if (useSquashStretch && visualsTransform != null)
                visualsTransform.localScale = new Vector3(baseScale.x * 0.85f, baseScale.y * 1.15f, baseScale.z);
        }
    }

    private void HandleGravity()
    {
        // Falling: apply stronger gravity for a snappier arc
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        // Rising but jump button released early: cut the jump short (variable jump height)
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Clamp fall speed
        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}