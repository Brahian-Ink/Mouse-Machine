using UnityEngine;

public class StompOnEnemy : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Stomp Check")]
    [SerializeField] private Transform stompCheck;
    [SerializeField] private float stompRadius = 0.12f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Stomp")]
    [SerializeField] private float minFallSpeed = -0.5f;
    [SerializeField] private float bounceY = 7.0f;
    [SerializeField] private int stompDamage = 999;

    [Header("Anti spam")]
    [SerializeField] private float stompLockTime = 0.15f; // evita doble stomp en el mismo contacto
    private float stompLockedUntil;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!stompCheck) return;
        if (Time.time < stompLockedUntil) return;

        // Solo si está cayendo de verdad
        if (rb.linearVelocity.y > minFallSpeed) return;

        Collider2D hit = Physics2D.OverlapCircle(stompCheck.position, stompRadius, enemyMask);
        if (!hit) return;

        // Asegura que venís desde arriba (no lateral)
        if (transform.position.y < hit.bounds.max.y - 0.05f) return;

        FrogHealth fh = hit.GetComponentInParent<FrogHealth>();
        if (fh == null) return;

        // Si la rana está invulnerable, no aplicar
        if (!fh.CanBeHit()) return;

        fh.TakeHit(stompDamage);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceY);

        stompLockedUntil = Time.time + stompLockTime;
    }

    void OnDrawGizmosSelected()
    {
        if (!stompCheck) return;
        Gizmos.DrawWireSphere(stompCheck.position, stompRadius);
    }
}
