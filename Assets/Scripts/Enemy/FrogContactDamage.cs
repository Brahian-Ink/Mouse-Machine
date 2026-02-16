using UnityEngine;

public class FrogContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float touchCooldown = 0.6f;

    [Header("Tags")]
    [SerializeField] private string ratTag = "Rat";
    [SerializeField] private string suitTag = "Suit";

    private float nextTouchTime;

    void OnTriggerEnter2D(Collider2D other) => TryDamage(other);
    void OnTriggerStay2D(Collider2D other) => TryDamage(other); // por si se queda adentro

    void OnCollisionEnter2D(Collision2D col) => TryDamage(col.collider);
    void OnCollisionStay2D(Collision2D col) => TryDamage(col.collider);

    private void TryDamage(Collider2D other)
    {
        if (Time.time < nextTouchTime) return;

        // Rata
        if (other.CompareTag(ratTag))
        {
            if (ApplyDamage(other))
                nextTouchTime = Time.time + touchCooldown;

            return;
        }

        // Suit solo si está ocupado
        if (other.CompareTag(suitTag))
        {
            SuitVehicle sv = other.GetComponent<SuitVehicle>();
            if (sv == null) sv = other.GetComponentInParent<SuitVehicle>();
            if (sv == null) return;

            if (sv.state != SuitVehicle.SuitState.Occupied) return;

            if (ApplyDamage(other))
                nextTouchTime = Time.time + touchCooldown;
        }
    }

    private bool ApplyDamage(Collider2D other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return false;

        ph.TakeDamage(touchDamage);
        return true;
    }
}
