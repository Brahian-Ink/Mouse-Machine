using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlowBullet : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2.0f;

    [Header("Hit")]
    [SerializeField] private LayerMask hitMask = ~0; // qué layers pueden destruir la bala (paredes, player, etc.)
    [SerializeField] private bool destroyOnHit = true;

    [Header("Ignore suit when empty")]
    [SerializeField] private string suitTag = "Suit";

    [Header("VFX")]
    [SerializeField] private GameObject impactVfxPrefab;

    private Rigidbody2D rb;
    private float deathTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        deathTime = Time.time + lifeTime;
    }

    void Update()
    {
        if (Time.time >= deathTime)
            Destroy(gameObject);
    }

    // Llamalo desde ScientistAI al instanciar
    public void Init(Vector2 dir)
    {
        dir = dir.normalized;
        if (rb != null) rb.linearVelocity = dir * speed;
        transform.right = dir;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanHit(other)) return;

        // Si tocó suit vacío: NO impacta, lo atraviesa
        if (IsEmptySuit(other))
        {
            IgnoreSuitCollision(other);
            return;
        }

        Impact(other.ClosestPoint(transform.position));

        if (destroyOnHit)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!CanHit(collision.collider)) return;

        if (IsEmptySuit(collision.collider))
        {
            IgnoreSuitCollision(collision.collider);
            return;
        }

        Vector2 point = collision.GetContact(0).point;
        Impact(point);

        if (destroyOnHit)
            Destroy(gameObject);
    }

    private bool CanHit(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return false;
        return true;
    }

    private bool IsEmptySuit(Collider2D other)
    {
        if (!other.CompareTag(suitTag)) return false;

        // Puede estar en el mismo GO o en el parent
        SuitVehicle sv = other.GetComponent<SuitVehicle>();
        if (sv == null) sv = other.GetComponentInParent<SuitVehicle>();
        if (sv == null) return false;

        return sv.state == SuitVehicle.SuitState.Empty;
    }

    private void IgnoreSuitCollision(Collider2D suitCol)
    {
        // Evita que siga triggereando y “molestando”
        var myCol = GetComponent<Collider2D>();
        if (myCol != null && suitCol != null)
            Physics2D.IgnoreCollision(myCol, suitCol, true);
    }

    private void Impact(Vector2 point)
    {
        if (impactVfxPrefab != null)
            Instantiate(impactVfxPrefab, point, Quaternion.identity);
    }
}
