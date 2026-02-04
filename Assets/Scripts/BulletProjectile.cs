using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private float lifeTime = 2.0f;

    [Header("Impact")]
    [SerializeField] private LayerMask hitMask = ~0; // todo
    [SerializeField] private bool destroyOnTrigger = true;

    [Header("Audio")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private Vector2 impactPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 impactVolumeRange = new Vector2(0.8f, 1.0f);

    [Header("Optional VFX")]
    [SerializeField] private GameObject impactVfxPrefab;

    float deathTime;

    void OnEnable()
    {
        deathTime = Time.time + lifeTime;
    }

    void Update()
    {
        if (Time.time >= deathTime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // filtrar por layer
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // Evitar que choque con triggers de UI o cosas raras: si querés, filtralo por tag/layer
        Impact(other.ClosestPoint(transform.position));

        if (destroyOnTrigger)
            Destroy(gameObject);
    }

    void Impact(Vector2 point)
    {
        // VFX
        if (impactVfxPrefab != null)
            Instantiate(impactVfxPrefab, point, Quaternion.identity);

        // SFX: se reproduce aunque la bala se destruya
        PlayImpactSfx(point);
    }

    void PlayImpactSfx(Vector2 worldPos)
    {
        if (impactClips == null || impactClips.Length == 0) return;

        var clip = impactClips[Random.Range(0, impactClips.Length)];
        float pitch = Random.Range(impactPitchRange.x, impactPitchRange.y);
        float vol = Random.Range(impactVolumeRange.x, impactVolumeRange.y);

        // Audio “one shot” en el mundo
        AudioSource.PlayClipAtPoint(clip, worldPos, vol);
        // Nota: PlayClipAtPoint crea un GO temporal. Para jam está perfecto.
    }
}
