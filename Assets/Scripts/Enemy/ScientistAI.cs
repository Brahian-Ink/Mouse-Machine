using UnityEngine;

public class ScientistAI : MonoBehaviour
{
    private enum State { Patrol, Combat }

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Targets")]
    [SerializeField] private string ratTag = "Rat";
    [SerializeField] private string suitTag = "Suit";

    private Transform rat;
    private Transform suit;
    private Transform target;

    private Animator suitAnim;
    private SuitVehicle suitVehicle;

    [Header("AI")]
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float combatRange = 7f;

    [Header("Line of sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask losMask;

    [Header("Patrol (walk/stop)")]
    [SerializeField] private float patrolSpeed = 1.2f;
    [SerializeField] private Vector2 walkTimeRange = new Vector2(1.2f, 2.2f);
    [SerializeField] private Vector2 shortStopTimeRange = new Vector2(0.35f, 0.75f);
    [SerializeField] private Vector2 longStopTimeRange = new Vector2(1.6f, 2.8f);

    [Header("Walls")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallCheckDistance = 0.25f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.15f, 0f);

    [Header("Combat Movement")]
    [SerializeField] private float combatSpeed = 1.4f;

    [Header("Shooting")]
    [SerializeField] private float shootCooldown = 3.0f;
    [SerializeField] private float shootWindup = 0.12f;
    [SerializeField] private string shootTrigger = "Shoot";
    [SerializeField] private string armedParam = "Armed";
    [SerializeField] private string speedParam = "Speed";

    private State state = State.Patrol;

    private float patrolDir = 1f;
    private float patrolTimer;
    private bool patrolWalking = true;
    private bool nextStopIsLong = false;

    private float shootTimer = 0f;
    private float strafeDir = 1f;
    private float strafeTimer = 0f;

    // Facing control independiente de la velocidad
    private float facingSign = 1f;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();

        CacheTargets();
        ResetPatrolPhase();

        facingSign = transform.localScale.x >= 0 ? 1f : -1f;
        ApplyFacing();
    }

    private void Update()
    {
        CacheTargets();
        ResolveTarget();

        if (!target || !target.gameObject.activeInHierarchy) return;

        float dist = Vector2.Distance(transform.position, target.position);
        bool sees = dist <= detectRange && (!requireLineOfSight || HasLineOfSightSmart(target));

        if (state == State.Patrol && sees && dist <= combatRange) state = State.Combat;
        else if (state == State.Combat && (!sees || dist > detectRange * 1.25f)) state = State.Patrol;

        if (state == State.Patrol)
        {
            PatrolTick();
            // En patrulla el facing sigue al movimiento
            if (rb.linearVelocity.x != 0f)
            {
                facingSign = Mathf.Sign(rb.linearVelocity.x);
                ApplyFacing();
            }
        }
        else
        {
            // En combate el facing SIEMPRE mira al target, aunque te muevas en otra dirección
            FaceTargetX(target.position.x);
            CombatTick(sees);
        }

        if (anim) anim.SetFloat(speedParam, Mathf.Abs(rb.linearVelocity.x));
    }

    private void PatrolTick()
    {
        if (anim) anim.SetBool(armedParam, false);

        if (patrolWalking && IsWallAhead())
        {
            FlipPatrolDir();
            patrolWalking = false;
            patrolTimer = Random.Range(shortStopTimeRange.x, shortStopTimeRange.y);
        }

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            if (patrolWalking)
            {
                patrolWalking = false;
                patrolTimer = Random.Range(
                    nextStopIsLong ? longStopTimeRange.x : shortStopTimeRange.x,
                    nextStopIsLong ? longStopTimeRange.y : shortStopTimeRange.y
                );
                nextStopIsLong = !nextStopIsLong;
            }
            else
            {
                patrolWalking = true;
                patrolTimer = Random.Range(walkTimeRange.x, walkTimeRange.y);
            }
        }

        rb.linearVelocity = new Vector2(patrolWalking ? patrolDir * patrolSpeed : 0f, rb.linearVelocity.y);
    }

    private void CombatTick(bool seesTarget)
    {
        if (anim) anim.SetBool(armedParam, true);
        if (!target) return;

        shootTimer -= Time.deltaTime;

        // Se mueve (no se queda quieto), pero el sprite sigue mirando al target por FaceTargetX
        Reposition();

        // Ya que el facing está clavado al target, este check ya no se rompe en cooldown
        if (seesTarget && shootTimer <= 0f && IsTargetInFront())
        {
            float dirToTarget = Mathf.Sign(target.position.x - transform.position.x);
            shootTimer = shootCooldown;
            StartCoroutine(ShootAfterWindup(dirToTarget));
        }
    }

    private void Reposition()
    {
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = Random.Range(0.35f, 0.75f);
            strafeDir = Random.value < 0.5f ? -1f : 1f;
        }

        rb.linearVelocity = new Vector2(strafeDir * combatSpeed, rb.linearVelocity.y);

        float checkDir = Mathf.Sign(rb.linearVelocity.x);
        Vector2 origin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * checkDir, wallCheckOffset.y);

        if (Physics2D.Raycast(origin, Vector2.right * checkDir, wallCheckDistance, wallMask))
        {
            strafeDir *= -1f;
            rb.linearVelocity = new Vector2(strafeDir * combatSpeed, rb.linearVelocity.y);
        }
    }

    private System.Collections.IEnumerator ShootAfterWindup(float dirToTarget)
    {
        // Facing ya está alineado al target, pero lo reforzamos por seguridad
        if (target) FaceTargetX(target.position.x);

        if (shootWindup > 0f)
            yield return new WaitForSeconds(shootWindup);

        if (anim && !string.IsNullOrEmpty(shootTrigger))
            anim.SetTrigger(shootTrigger);

        if (bulletPrefab && muzzle)
        {
            GameObject b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);

            var sb = b.GetComponent<SlowBullet>();
            if (sb != null) sb.Init(dirToTarget < 0 ? Vector2.left : Vector2.right);
            else
            {
                var rbB = b.GetComponent<Rigidbody2D>();
                if (rbB) rbB.linearVelocity = new Vector2((dirToTarget < 0 ? -1 : 1) * 8f, 0f);
            }
        }
    }

    private void CacheTargets()
    {
        if (!rat)
        {
            var go = GameObject.FindGameObjectWithTag(ratTag);
            if (go) rat = go.transform;
        }

        if (!suit)
        {
            var go = GameObject.FindGameObjectWithTag(suitTag);
            if (go)
            {
                suit = go.transform;
                suitAnim = suit.GetComponent<Animator>();
                suitVehicle = suit.GetComponent<SuitVehicle>();
            }
        }
    }

    private void ResolveTarget()
    {
        bool suitOccupied = IsSuitOccupied();
        target = (suit && suitOccupied) ? suit : rat;
    }

    private bool IsSuitOccupied()
    {
        if (!suit) return false;

        if (suitVehicle != null)
            return suitVehicle.state == SuitVehicle.SuitState.Occupied;

        if (suitAnim)
        {
            if (HasParam(suitAnim, "Occupied")) return suitAnim.GetBool("Occupied");
            if (HasParam(suitAnim, "Empty")) return !suitAnim.GetBool("Empty");
        }

        return false;
    }

    private bool IsTargetInFront()
    {
        if (!target) return false;
        float dirToTarget = Mathf.Sign(target.position.x - transform.position.x);
        return facingSign == dirToTarget;
    }

    private bool HasLineOfSightSmart(Transform t)
    {
        Vector2 from = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
        Vector2 to = t.position;
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);

        RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, dist, losMask);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool suitOccupied = IsSuitOccupied();

        foreach (var hit in hits)
        {
            if (!hit.collider) continue;

            Transform h = hit.transform;

            if (!suitOccupied && suit && (h == suit || h.root == suit.root))
                continue;

            if (h == t || h.IsChildOf(t) || h.root == t.root)
                return true;

            return false;
        }

        return false;
    }

    private bool HasParam(Animator a, string paramName)
    {
        foreach (var p in a.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private bool IsWallAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * patrolDir, wallCheckOffset.y);
        return Physics2D.Raycast(origin, Vector2.right * patrolDir, wallCheckDistance, wallMask);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state != State.Patrol) return;
        if (((1 << col.gameObject.layer) & wallMask.value) == 0) return;

        FlipPatrolDir();
        patrolWalking = false;
        patrolTimer = Random.Range(shortStopTimeRange.x, shortStopTimeRange.y);
    }

    private void FlipPatrolDir() => patrolDir *= -1f;

    private void ResetPatrolPhase()
    {
        patrolWalking = true;
        nextStopIsLong = false;
        patrolTimer = Random.Range(walkTimeRange.x, walkTimeRange.y);
    }

    private void FaceTargetX(float targetX)
    {
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (dir == 0f) return;
        facingSign = dir;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        transform.localScale = new Vector3(facingSign, 1f, 1f);
    }
}
