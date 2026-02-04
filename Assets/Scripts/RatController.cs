using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RatController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4.5f;
    [SerializeField] private FootstepDust footstepDust;

    [Header("Jump")]
    public float jumpForce = 6.5f;
    [SerializeField] private RatJumpSfx jumpSfx;

    [Header("Wall Jump (Rat only)")]
    [SerializeField] private bool enableWallJump = true;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.12f, 0.6f);
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private float wallSlideSpeed = 2.0f;
    [SerializeField] private float wallJumpForceX = 6.5f;
    [SerializeField] private float wallJumpForceY = 7.5f;
    [SerializeField] private float wallJumpLockTime = 0.12f;
    [SerializeField] private float wallCoyoteTime = 0.12f;

    [Header("Anti single-wall climb")]
    [SerializeField] private float sameWallRegrabBlockTime = 0.20f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Audio Footsteps")]
    [SerializeField] private RatFootstepsSfx footsteps;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float inputX;
    bool grounded;

    // Wall
    bool onWallLeft;
    bool onWallRight;
    int wallSide; // -1 pared izquierda, +1 pared derecha
    float wallCoyoteTimer;
    float wallJumpLockTimer;

    // Anti re-grab same wall
    float blockSameWallUntil;
    int blockedWallSide; // -1 left, +1 right, 0 none

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (footstepDust == null)
            footstepDust = GetComponent<FootstepDust>();
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // ---------- WALL CHECK ----------
        if (enableWallJump)
        {
            onWallLeft = wallCheckLeft != null &&
                Physics2D.OverlapBox(wallCheckLeft.position, wallCheckSize, 0f, wallLayer);

            onWallRight = wallCheckRight != null &&
                Physics2D.OverlapBox(wallCheckRight.position, wallCheckSize, 0f, wallLayer);

            // Bloqueo para evitar escalar una sola pared
            if (Time.time < blockSameWallUntil)
            {
                if (blockedWallSide == -1) onWallLeft = false;
                else if (blockedWallSide == 1) onWallRight = false;
            }

            bool touchingWall = !grounded && (onWallLeft || onWallRight);

            // Coyote time (solo para saltar)
            if (touchingWall)
            {
                wallSide = onWallRight ? 1 : -1;
                wallCoyoteTimer = wallCoyoteTime;
            }
            else
            {
                wallCoyoteTimer -= Time.deltaTime;
            }

            // WALL SLIDE físico
            if (touchingWall && rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector2(
                    0f, // evita que se "pegue" con input
                    Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed)
                );
            }
        }
        else
        {
            wallCoyoteTimer = -1f;
        }

        // ---------- WALL SLIDE ANIM (instantáneo, sin coyote) ----------
        bool wallSliding =
            enableWallJump &&
            !grounded &&
            (onWallLeft || onWallRight) &&
            rb.linearVelocity.y <= 0.1f;

        if (anim != null)
            anim.SetBool("WallSliding", wallSliding);

        // Orientación visual: mirar hacia la pared
        if (wallSliding)
            sr.flipX = onWallLeft;

        // ---------- JUMP ----------
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                DoGroundJump();
            }
            else if (enableWallJump && wallCoyoteTimer > 0f)
            {
                DoWallJump();
            }
        }

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.deltaTime;

        // ---------- ANIM BASE ----------
        float speed01 = Mathf.Abs(inputX);
        if (anim != null)
        {
            anim.SetFloat("Speed", speed01);
            anim.SetBool("Grounded", grounded);
        }

        // Footstep SFX
        if (footsteps != null)
        {
            footsteps.CurrentSpeed01 = speed01;
            footsteps.IsGrounded = grounded;
        }

        // Footstep Dust
        if (footstepDust != null)
        {
            footstepDust.Speed01 = speed01;
            footstepDust.Grounded = grounded;
        }

        // FLIP normal (solo si NO está en wallslide y no está en lock)
        if (!wallSliding && inputX != 0 && wallJumpLockTimer <= 0f)
            sr.flipX = inputX < 0;
    }

    void FixedUpdate()
    {
        Vector2 v = rb.linearVelocity;

        // Bloquea control X un instante tras walljump
        if (wallJumpLockTimer <= 0f)
            v.x = inputX * moveSpeed;

        rb.linearVelocity = v;
    }

    void DoGroundJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (jumpSfx != null) jumpSfx.PlayJump();
    }

    void DoWallJump()
    {
        // Empuja al lado opuesto de la pared
        float dirAway = -wallSide;

        rb.linearVelocity = new Vector2(dirAway * wallJumpForceX, wallJumpForceY);

        // Lock para que no se re-pegue raro
        wallJumpLockTimer = wallJumpLockTime;
        wallCoyoteTimer = 0f;

        // Bloquear re-agarre de la misma pared (evita "escalar" una sola pared)
        blockedWallSide = wallSide;
        blockSameWallUntil = Time.time + sameWallRegrabBlockTime;

        // mirar hacia donde sale
        sr.flipX = dirAway < 0f;

        if (jumpSfx != null) jumpSfx.PlayJump();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        if (wallCheckLeft != null)
            Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckSize);

        if (wallCheckRight != null)
            Gizmos.DrawWireCube(wallCheckRight.position, wallCheckSize);
    }
}
