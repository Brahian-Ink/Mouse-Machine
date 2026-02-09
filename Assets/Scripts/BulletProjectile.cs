using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private float lifeTime = 2.0f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Impact")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Audio")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private Vector2 impactPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 impactVolumeRange = new Vector2(0.8f, 1.0f);

    [Header("Optional VFX")]
    [SerializeField] private GameObject impactVfxPrefab;

    private float deathTime;
    private bool hasHit;

    void OnEnable()
    {
        deathTime = Time.time + lifeTime;
        hasHit = false;
    }

    void Update()
    {
        if (Time.time >= deathTime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // Si chocás con triggers “decorativos” (por ejemplo zonas) esto evita impactos falsos.
        // Dejalo si lo necesitás; si no, podés sacarlo.
        if (other.isTrigger && other.GetComponent<EnemyHealth>() == null && other.GetComponentInParent<EnemyHealth>() == null)
            return;

        ProcessHit(other, other.ClosestPoint(transform.position));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        if (((1 << collision.gameObject.layer) & hitMask) == 0) return;

        var other = collision.collider;
        Vector2 point = collision.GetContact(0).point;

        ProcessHit(other, point);
    }

    private void ProcessHit(Collider2D other, Vector2 point)
    {
        hasHit = true;

        // Aplicar daño si es enemigo (sirve aunque el collider esté en un hijo)
        var eh = other.GetComponent<EnemyHealth>();
        if (eh == null) eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null) eh.TakeHit(damage);

        Impact(point);

        if (destroyOnHit)
            Destroy(gameObject);
    }

    void Impact(Vector2 point)
    {
        if (impactVfxPrefab != null)
            Instantiate(impactVfxPrefab, point, Quaternion.identity);

        PlayImpactSfx(point);
    }

    void PlayImpactSfx(Vector2 worldPos)
    {
        if (impactClips == null || impactClips.Length == 0) return;

        var clip = impactClips[Random.Range(0, impactClips.Length)];
        float pitch = Random.Range(impactPitchRange.x, impactPitchRange.y);
        float vol = Random.Range(impactVolumeRange.x, impactVolumeRange.y);

        var go = new GameObject("ImpactSfxTemp");
        go.transform.position = worldPos;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.pitch = pitch;
        src.volume = vol;
        src.spatialBlend = 0f;
        src.Play();

        Destroy(go, clip.length / Mathf.Max(0.01f, pitch));
    }
}
