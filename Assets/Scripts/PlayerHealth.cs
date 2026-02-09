using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 3;

    [Header("Invulnerability")]
    [SerializeField] private float iFrames = 0.6f;

    [Header("Flash")]
    [SerializeField] private bool usePropertyBlockFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashAlpha = 0.25f;
    [SerializeField] private int flashTimes = 3;
    [SerializeField] private float flashInterval = 0.06f;

    [Header("Death")]
    [SerializeField] private float reloadDelay = 0.15f;

    private int hp;
    private bool dead;
    private float invulnUntil;

    private SpriteRenderer[] srs;
    private MaterialPropertyBlock mpb;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private Coroutine flashRoutine;

    public bool Dead => dead;

    void Awake()
    {
        maxHp = Mathf.Max(1, maxHp);
        hp = maxHp;

        srs = GetComponentsInChildren<SpriteRenderer>(true);
        mpb = new MaterialPropertyBlock();
    }

    public void TakeDamage(int dmg = 1)
    {
        if (dead) return;
        if (Time.time < invulnUntil) return;

        dmg = Mathf.Max(1, dmg);
        hp -= dmg;
        invulnUntil = Time.time + iFrames;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());

        if (hp <= 0)
        {
            hp = 0;
            dead = true;
            Debug.Log("MURIO");
            StartCoroutine(ReloadRoom());
        }
    }

    private IEnumerator ReloadRoom()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            if (usePropertyBlockFlash)
                sr.SetPropertyBlock(null);
            else
                sr.color = original[i];
        }
    }
}
