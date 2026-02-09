using UnityEngine;

public class SuitWeaponController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Return;

    [Header("Shoot")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float fireCooldown = 0.15f;
    [SerializeField] private SuitVehicle suitVehicle;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootClips;
    [SerializeField] private Vector2 shootPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 shootVolumeRange = new Vector2(0.8f, 1.0f);

    [Header("Facing (optional)")]
    [SerializeField] private SpriteRenderer suitSprite;
    [SerializeField] private bool useSpriteFlipX = true;

    [SerializeField] private Vector2 muzzleBaseLocalPos = new Vector2(-0.6f, 0.2f);

    // Si tu flipX está invertido respecto a izquierda/derecha, activá esto.
    [SerializeField] private bool invertFlipLogic = true;

    private float nextFireTime;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (suitSprite == null) suitSprite = GetComponentInChildren<SpriteRenderer>();
        if (suitVehicle == null) suitVehicle = GetComponent<SuitVehicle>();
    }

    void Update()
    {
        // Solo dispara si el traje está ocupado.
        if (suitVehicle != null && suitVehicle.state != SuitVehicle.SuitState.Occupied)
            return;

        if (Time.time < nextFireTime) return;

        if (Input.GetKeyDown(fireKey))
        {
            FireOnce();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void LateUpdate()
    {
        if (muzzle == null || suitSprite == null) return;

        // Mantiene el muzzle del lado correcto según el facing real.
        // Se usa la misma lógica del facing que se usa para la dirección de disparo.
        Vector2 pos = muzzleBaseLocalPos;

        bool isFacingLeft = suitSprite.flipX;
        if (invertFlipLogic) isFacingLeft = !isFacingLeft;

        pos.x = Mathf.Abs(muzzleBaseLocalPos.x) * (isFacingLeft ? -1f : 1f);

        muzzle.localPosition = pos;
    }

    void FireOnce()
    {
        if (muzzle == null || bulletPrefab == null) return;

        Vector2 dir = GetFacingDir();

        GameObject bulletGO = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);

        // Evita que la bala choque con el suit que la dispara.
        var bulletCol = bulletGO.GetComponent<Collider2D>();
        var suitCol = GetComponent<Collider2D>();

        if (bulletCol != null && suitCol != null)
            Physics2D.IgnoreCollision(bulletCol, suitCol, true);

        if (bulletCol != null)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(bulletCol, col, true);
        }

        // Velocidad de bala.
        var rb = bulletGO.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * bulletSpeed;

        // Orientación visual de la bala, si el sprite usa el eje right.
        bulletGO.transform.right = dir;

        PlayShootSfx();
    }

    Vector2 GetFacingDir()
    {
        // Si se usa flipX del sprite como fuente de facing, se respeta invertFlipLogic.
        if (useSpriteFlipX && suitSprite != null)
        {
            bool isFacingLeft = suitSprite.flipX;
            if (invertFlipLogic) isFacingLeft = !isFacingLeft;

            return isFacingLeft ? Vector2.left : Vector2.right;
        }

        // Fallback: usa la escala global.
        float sx = transform.lossyScale.x;
        return sx < 0 ? Vector2.left : Vector2.right;
    }

    void PlayShootSfx()
    {
        if (audioSource == null) return;
        if (shootClips == null || shootClips.Length == 0) return;

        var clip = shootClips[Random.Range(0, shootClips.Length)];
        audioSource.pitch = Random.Range(shootPitchRange.x, shootPitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(shootVolumeRange.x, shootVolumeRange.y));
    }
}
