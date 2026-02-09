using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 5;

    [Header("Player Bullet")]
    [SerializeField] private bool useLayerFilter = false;
    [SerializeField] private LayerMask playerBulletLayer;
    [SerializeField] private string[] playerBulletTags = { "PlayerBullet", "BulletPlayer", "Bullet" };
    [SerializeField] private bool destroyBulletOnHit = true;

    [Header("Hit Flash (Renderer)")]
    [SerializeField] private bool usePropertyBlockFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashAlpha = 0.25f;
    [SerializeField] private int flashTimes = 3;
    [SerializeField] private float flashInterval = 0.06f;

    [Header("Death VFX")]
    [SerializeField] private GameObject deathParticlesPrefab;

    [Header("Debug")]
    [SerializeField] private bool logContacts = false;
    [SerializeField] private bool logFlashTargets = false;

    private int hp;
    private bool dead;

    private SpriteRenderer[] srs;
    private Coroutine flashRoutine;

    // PropertyBlock (no lo pisa el Animator)
    private MaterialPropertyBlock mpb;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    void Awake()
    {
        hp = Mathf.Max(1, maxHp);

        // IMPORTANTÍSIMO: agarrar TODOS los SpriteRenderer del enemigo (incluye hijos)
        srs = GetComponentsInChildren<SpriteRenderer>(true);

        if (logFlashTargets)
        {
            Debug.Log($"[EnemyHealth] {name} SpriteRenderers found: {srs.Length}");
            for (int i = 0; i < srs.Length; i++)
                Debug.Log($"  SR[{i}] -> {srs[i].name}");
        }

        mpb = new MaterialPropertyBlock();
    }

    public void TakeHit(int damage = 1)
    {
        if (dead) return;

        hp -= Mathf.Max(1, damage);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlashRoutine());

        if (hp <= 0) Die();
    }

    void Die()
    {
        dead = true;

        if (deathParticlesPrefab != null)
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    IEnumerator HitFlashRoutine()
    {
        if (srs == null || srs.Length == 0) yield break;

        // Guardar estado visual
        Color[] originalColors = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
        {
            if (!srs[i]) continue;
            originalColors[i] = srs[i].color;
        }

        Color flashed = flashColor;
        flashed.a = flashAlpha;

        for (int n = 0; n < flashTimes; n++)
        {
            ApplyFlash(flashed);
            yield return new WaitForSeconds(flashInterval);

            ClearFlash(originalColors);
            yield return new WaitForSeconds(flashInterval);
        }

        ClearFlash(originalColors);
        flashRoutine = null;
    }

    void ApplyFlash(Color c)
    {
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (!sr) continue;

            if (usePropertyBlockFlash)
            {
                sr.GetPropertyBlock(mpb);
                mpb.SetColor(ColorProp, c);
                sr.SetPropertyBlock(mpb);
            }
            else
            {
                sr.color = c;
            }
        }
    }

    void ClearFlash(Color[] originalColors)
    {
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (!sr) continue;

            if (usePropertyBlockFlash)
            {
                // Limpia override
                sr.SetPropertyBlock(null);
            }
            else
            {
                sr.color = originalColors[i];
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) => TryHitFrom(other.gameObject);
    void OnCollisionEnter2D(Collision2D col) => TryHitFrom(col.gameObject);

    void TryHitFrom(GameObject otherGO)
    {
        if (dead) return;

        if (logContacts)
            Debug.Log($"[EnemyHealth] Contact: {name} <- {otherGO.name} | tag={otherGO.tag} layer={LayerMask.LayerToName(otherGO.layer)}");

        // Ignora balas del científico
        if (otherGO.GetComponent<SlowBullet>() != null) return;

        if (!IsPlayerBullet(otherGO)) return;

        TakeHit(1);

        if (destroyBulletOnHit)
            Destroy(otherGO);
    }

    bool IsPlayerBullet(GameObject go)
    {
        // Tag primero (tu bala está en Default)
        if (playerBulletTags != null)
        {
            for (int i = 0; i < playerBulletTags.Length; i++)
            {
                string t = playerBulletTags[i];
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t))
                    return true;
            }
        }

        // Layer opcional
        if (useLayerFilter && playerBulletLayer.value != 0)
            return ((1 << go.layer) & playerBulletLayer.value) != 0;

        return false;
    }
}
