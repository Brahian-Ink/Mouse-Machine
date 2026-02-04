using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Jump")]
    public float jumpForce = 7.5f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float cutJumpMultiplier = 0.5f; // 0.5 corta bastante
    [SerializeField] private JumpSfx jumpSfx;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.08f;
    public LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private FootstepsSfx footsteps;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float inputX;
    bool grounded;

    float coyoteTimer;
    float jumpBufferTimer;


    [SerializeField] private LandingSfx landingSfx;

    bool wasGrounded;
    float lastYVelocity;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // Grounded
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Guardar velocidad vertical previa
        lastYVelocity = rb.linearVelocity.y;

        // Detectar grounded como ya hac�s
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Landing: aire -> suelo
        if (!wasGrounded && grounded)
        {
            float impact = Mathf.InverseLerp(-10f, -2f, lastYVelocity);
            impact = Mathf.Clamp01(impact);

            if (landingSfx != null)
                landingSfx.PlayLand(impact);
        }

        wasGrounded = grounded;

        // Timers
        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferTimer = jumpBuffer;
        else jumpBufferTimer -= Time.deltaTime;

        // Jump (buffer + coyote)
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            Jump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // Variable jump (cut)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cutJumpMultiplier);
        }

        // Anim params
        float speed01 = Mathf.Abs(inputX);
        anim.SetFloat("Speed", speed01);
        anim.SetBool("Grounded", grounded);
        anim.SetFloat("YVel", rb.linearVelocity.y);

        if (footsteps != null)
            footsteps.CurrentSpeed01 = grounded ? speed01 : 0f;

        // Flip
        if (inputX != 0)
            sr.flipX = inputX > 0;
    }

    void FixedUpdate()
    {
        var v = rb.linearVelocity;
        v.x = inputX * moveSpeed;
        rb.linearVelocity = v;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (jumpSfx != null)
            jumpSfx.PlayJump();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
