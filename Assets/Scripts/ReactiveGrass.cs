using UnityEngine;

public class ReactiveGrass : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator anim;

    [Header("Base")]
    [SerializeField] private float baseAnimSpeed = 1f;

    [Header("Boost Speeds")]
    [SerializeField] private float ratBoostSpeed = 2.5f;
    [SerializeField] private float suitBoostSpeed = 1.6f;

    [Header("Only boost if actor is moving")]
    [SerializeField] private float minAbsVelocityX = 0.2f;

    [Header("Return")]
    [SerializeField] private float returnDelay = 0.05f;
    [SerializeField] private float returnLerpSpeed = 10f;

    float targetSpeed;
    float returnAtTime;

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        targetSpeed = baseAnimSpeed;
        if (anim != null) anim.speed = baseAnimSpeed;
    }

    void Update()
    {
        if (anim == null) return;

        // Volver a base pasado el delay
        if (Time.time >= returnAtTime)
            targetSpeed = baseAnimSpeed;

        // Suavizado (evita saltos raros)
        anim.speed = Mathf.Lerp(anim.speed, targetSpeed, Time.deltaTime * returnLerpSpeed);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (anim == null) return;

        // Solo si se mueve (no parado dentro del pasto)
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        if (Mathf.Abs(rb.linearVelocity.x) < minAbsVelocityX) return;

        if (other.CompareTag("Rat"))
        {
            targetSpeed = ratBoostSpeed;
            returnAtTime = Time.time + returnDelay;
        }
        else if (other.CompareTag("Player"))
        {
            targetSpeed = suitBoostSpeed;
            returnAtTime = Time.time + returnDelay;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Al salir, que vuelva (con el suavizado de Update)
        returnAtTime = Time.time; // permite volver ya
    }
}
