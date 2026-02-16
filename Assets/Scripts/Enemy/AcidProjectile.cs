using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AcidProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Life")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Impact")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private GameObject impactVfxPrefab;

    private Rigidbody2D rb;
    private float dieAt;
    private bool impacted;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        dieAt = Time.time + lifeTime;
        impacted = false;
    }

    void Update()
    {
        if (Time.time >= dieAt)
            Destroy(gameObject);
    }

    public void LaunchTo(Vector2 targetPos, float timeToTarget)
    {
        if (timeToTarget <= 0.05f) timeToTarget = 0.05f;

        Vector2 start = rb.position;
        Vector2 to = targetPos - start;

        float t = timeToTarget;
        float g = Physics2D.gravity.y * rb.gravityScale;

        float vx = to.x / t;
        float vy = (to.y - 0.5f * g * t * t) / t;

        rb.linearVelocity = new Vector2(vx, vy);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (impacted) return;
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;
        Impact(other);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (impacted) return;
        if (((1 << col.gameObject.layer) & hitMask) == 0) return;
        Impact(col.collider);
    }

    private void Impact(Collider2D other)
    {
        impacted = true;

        // Regla especial: Suit solo recibe daño si está Occupied
        SuitVehicle suit = other.GetComponent<SuitVehicle>();
        if (suit == null) suit = other.GetComponentInParent<SuitVehicle>();

        if (suit != null)
        {
            if (suit.state == SuitVehicle.SuitState.Occupied)
            {
                PlayerHealth ph = other.GetComponent<PlayerHealth>();
                if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
            }
        }
        else
        {
            // Si NO es suit, entonces puede ser la rata u otro player
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
        }

        if (impactVfxPrefab != null)
            Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
