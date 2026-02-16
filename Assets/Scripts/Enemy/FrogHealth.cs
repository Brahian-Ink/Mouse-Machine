using System.Collections;
using UnityEngine;

public class FrogHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 1;

    [Header("Invulnerability")]
    [Tooltip("Si querés invulnerable solo mientras parpadea, dejá esto en 0.")]
    [SerializeField] private float extraInvulnTime = 0f; // extra opcional

    [Header("Flash")]
    [SerializeField] private bool usePropertyBlockFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashAlpha = 0.25f;
    [SerializeField] private int flashTimes = 3;
    [SerializeField] private float flashInterval = 0.06f;

    [Header("Death")]
    [SerializeField] private GameObject deathVfxPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hitClips;
    [SerializeField] private AudioClip[] deathClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.0f);

    private int hp;
    private bool dead;
    private float invulnUntil;

    private SpriteRenderer[] srs;
    private MaterialPropertyBlock mpb;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private Coroutine flashRoutine;

    public bool Dead => dead;
    public bool CanBeHit() => !dead && Time.time >= invulnUntil;

    void Awake()
    {
        hp = Mathf.Max(1, maxHp);
        srs = GetComponentsInChildren<SpriteRenderer>(true);
        mpb = new MaterialPropertyBlock();

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeHit(int dmg = 1)
    {
        if (dead) return;
        if (Time.time < invulnUntil) return;

        dmg = Mathf.Max(1, dmg);
        hp -= dmg;

        PlayOneShot(hitClips);

        // Invuln = duración total del flash (mientras parpadea) + extra opcional
        float flashTotal = flashTimes * flashInterval * 2f;
        invulnUntil = Time.time + flashTotal + Mathf.Max(0f, extraInvulnTime);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());

        if (hp <= 0)
            Die();
    }

    public void Die()
    {
        if (dead) return;
        dead = true;

        PlayOneShot(deathClips);

        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.05f);
    }

    private void PlayOneShot(AudioClip[] clips)
    {
        if (!audioSource) return;
        if (clips == null || clips.Length == 0) return;

        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }

    private IEnumerator FlashRoutine()
    {
        if (srs == null || srs.Length == 0) yield break;

        Color[] original = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
            original[i] = srs[i] ? srs[i].color : Color.white;

        Color flashed = flashColor;
        flashed.a = flashAlpha;

        for (int n = 0; n < flashTimes; n++)
        {
            ApplyFlash(flashed);
            yield return new WaitForSeconds(flashInterval);

            ClearFlash(original);
            yield return new WaitForSeconds(flashInterval);
        }

        ClearFlash(original);
        flashRoutine = null;
    }

    private void ApplyFlash(Color c)
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

    private void ClearFlash(Color[] original)
    {
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (!sr) continue;

            if (usePropertyBlockFlash) sr.SetPropertyBlock(null);
            else sr.color = original[i];
        }
    }
}
