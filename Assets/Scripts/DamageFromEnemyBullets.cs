using UnityEngine;

public class DamageFromEnemyBullets : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SuitVehicle suitVehicle; // solo existe en el suit
    [SerializeField] private int damage = 1;

    void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (suitVehicle == null) suitVehicle = GetComponent<SuitVehicle>();
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnCollisionEnter2D(Collision2D col) => TryHit(col.collider);

    private void TryHit(Collider2D other)
    {
        if (playerHealth == null || playerHealth.Dead) return;

        // Solo bala enemiga
        if (other.GetComponent<SlowBullet>() == null) return;

        // Suit vacío: no recibe daño
        if (suitVehicle != null && suitVehicle.state == SuitVehicle.SuitState.Empty)
            return;

        playerHealth.TakeDamage(damage);
        // La destrucción + partículas las maneja la propia bala (SlowBullet) al impactar
    }
}
