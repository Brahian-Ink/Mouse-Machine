using UnityEngine;

public class FrogAI : MonoBehaviour
{
    private enum State { Patrol, Alert }

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform spitPoint;
    [SerializeField] private GameObject acidPrefab;

    [Header("Target")]
    [SerializeField] private string ratTag = "Rat";
    [SerializeField] private string suitTag = "Suit";
    private Transform rat;
    private Transform suit;
    private SuitVehicle suitVehicle;
    private Transform target;

    [Header("Detection")]
    [SerializeField] private float detectRange = 7f;
    [SerializeField] private LayerMask losMask;
    [SerializeField] private bool requireLineOfSight = false;

    [Header("Patrol Jump")]
    [SerializeField] private float patrolJumpInterval = 3f;
    [SerializeField] private float patrolJumpForceX = 3.0f;
    [SerializeField] private float patrolJumpForceY = 6.0f;

    [Header("Alert Jump")]
    [SerializeField] private float alertJumpInterval = 1.5f;
    [SerializeField] private float alertJumpForceX = 4.0f;
    [SerializeField] private float alertJumpForceY = 6.5f;

    [Header("Spit")]
    [SerializeField] private float spitCooldown = 2.2f;
    [SerializeField] private float spitChancePerAction = 0.35f;
    [SerializeField] private float spitWindup = 0.12f;
    [SerializeField] private float acidFlightTime = 0.55f;

    [Header("If can't jump in alert")]
    [SerializeField] private float spitOnlyRange = 5.5f; // si está en rango y no hay suelo adelante, spittea

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.12f;
    [SerializeField] private LayerMask groundMask;

    [Header("Ledge Avoid")]
    [SerializeField] private LayerMask groundAheadMask;
    [SerializeField] private float ledgeCheckForward = 0.6f;
    [SerializeField] private float ledgeCheckDown = 1.5f;
    [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0f, 0.2f);

    [Header("Anim Params")]
    [SerializeField] private string jumpBool = "Jump";
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] spitClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.0f);

    private State state = State.Patrol;
    private float nextActionTime;
    private float nextSpitTime;
    private bool grounded;

    private Vector2 lastKnownTargetPos;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        CacheTargets();
        nextActionTime = Time.time + patrolJumpInterval;

        if (groundAheadMask.value == 0) groundAheadMask = groundMask;
    }

    void Update()
    {
        CacheTargets();
        ResolveTarget();

        grounded = IsGrounded();

        if (!target || !target.gameObject.activeInHierarchy)
        {
            state = State.Patrol;
        }
        else
        {
            float dist = Vector2.Distance(transform.position, target.position);
            bool sees = dist <= detectRange && (!requireLineOfSight || HasLineOfSight(target));
            state = sees ? State.Alert : State.Patrol;
        }

        if (anim) anim.SetBool(jumpBool, !grounded);

        if (Time.time >= nextActionTime)
        {
            if (state == State.Patrol)
            {
                DoPatrolAction();
                nextActionTime = Time.time + patrolJumpInterval;
            }
            else
            {
                DoAlertAction();
                nextActionTime = Time.time + alertJumpInterval;
            }
        }
    }

    private void DoPatrolAction()
    {
        if (!grounded) return;

        float dir = FacingDir();

        // En patrulla, si no hay suelo, se da vuelta
        if (!HasGroundAhead(dir))
        {
            Flip();
            return;
        }

        Jump(dir * patrolJumpForceX, patrolJumpForceY);
    }

    private void DoAlertAction()
    {
        if (!grounded) return;
        if (!target) return;

        FaceTarget();
        float dir = FacingDir();

        float distToTarget = Vector2.Distance(transform.position, target.position);
        bool canJumpForward = HasGroundAhead(dir);

        // Si NO puede saltar pero está en rango: no se suicida ni se da vuelta, escupe
        if (!canJumpForward && distToTarget <= spitOnlyRange)
        {
            TrySpitSnapshot();
            return;
        }

        // Si puede saltar, decide si escupe o salta
        if (TrySpitSnapshot(forceChance: false))
            return;

        // Salto hacia el target solo si hay suelo
        if (!canJumpForward)
        {
            // si no hay suelo y no está en rango de spit, se repliega (se da vuelta)
            Flip();
            return;
        }

        Jump(dir * alertJumpForceX, alertJumpForceY);
    }

    private bool TrySpitSnapshot(bool forceChance = false)
    {
        if (!acidPrefab || !spitPoint) return false;
        if (Time.time < nextSpitTime) return false;

        // Chance
        if (!forceChance && Random.value >= spitChancePerAction)
            return false;

        nextSpitTime = Time.time + spitCooldown;

        lastKnownTargetPos = target ? (Vector2)target.position : (Vector2)transform.position;
        StartCoroutine(SpitRoutine(lastKnownTargetPos));

        return true;
    }

    private System.Collections.IEnumerator SpitRoutine(Vector2 targetPosSnapshot)
    {
        if (anim && !string.IsNullOrEmpty(attackTrigger))
            anim.SetTrigger(attackTrigger);

        PlayOneShot(spitClips);

        if (spitWindup > 0f)
            yield return new WaitForSeconds(spitWindup);

        if (!acidPrefab || !spitPoint) yield break;

        Vector2 start = spitPoint.position;
        GameObject go = Instantiate(acidPrefab, start, Quaternion.identity);

        var acid = go.GetComponent<AcidProjectile>();
        if (acid != null)
            acid.LaunchTo(targetPosSnapshot, acidFlightTime);
    }

    private void Jump(float vx, float vy)
    {
        rb.linearVelocity = new Vector2(vx, vy);
        PlayOneShot(jumpClips);
    }

    private void PlayOneShot(AudioClip[] clips)
    {
        if (!audioSource) return;
        if (clips == null || clips.Length == 0) return;

        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }

    private float FacingDir() => transform.localScale.x >= 0 ? 1f : -1f;

    private void Flip()
    {
        transform.localScale = new Vector3(-FacingDir(), 1f, 1f);
    }

    private void FaceTarget()
    {
        float dir = Mathf.Sign(target.position.x - transform.position.x);
        if (dir == 0) return;
        transform.localScale = new Vector3(dir, 1f, 1f);
    }

    private bool HasGroundAhead(float dir)
    {
        Vector2 origin = (Vector2)transform.position
                         + new Vector2(ledgeCheckOffset.x + ledgeCheckForward * dir, ledgeCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDown, groundAheadMask);
        return hit.collider != null;
    }

    private bool IsGrounded()
    {
        if (!groundCheck) return true;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
    }

    private bool HasLineOfSight(Transform t)
    {
        Vector2 from = transform.position;
        Vector2 to = t.position;
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.Raycast(from, dir, dist, losMask);
        if (!hit) return false;

        if (hit.transform == t) return true;
        if (hit.transform.IsChildOf(t)) return true;
        if (hit.transform.root == t.root) return true;

        return false;
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
                suitVehicle = suit.GetComponent<SuitVehicle>();
            }
        }
    }

    private void ResolveTarget()
    {
        bool suitOccupied = suitVehicle != null && suitVehicle.state == SuitVehicle.SuitState.Occupied;
        target = (suit && suitOccupied) ? suit : rat;
    }
}
