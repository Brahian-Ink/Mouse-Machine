using UnityEngine;

public class SuitVehicle : MonoBehaviour
{
    public enum SuitState { Empty, Occupied }

    [Header("State")]
    public SuitState state = SuitState.Empty;

    [Header("Refs")]
    [SerializeField] private Animator anim;
    [SerializeField] private MonoBehaviour suitMovementScript; 
    [SerializeField] private Transform ejectPoint;            
    [SerializeField] private CameraFollowBounds2D cameraFollow; 
    [SerializeField] private Transform suitCameraTarget;        
    [SerializeField] private Transform ratCameraTarget;        

    [Header("Interact")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject promptUI;             
    [SerializeField] private float ejectForceX = 4f;
    [SerializeField] private float ejectForceY = 7f;

    [Header("Re-enter block")]
    [SerializeField] private float reenterBlockTime = 0.35f;
    private float blockEnterUntil;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] enterSuitClips;
    [SerializeField] private AudioClip[] exitSuitClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.0f);

    [Header("Physics Materials")]
    [SerializeField] private Collider2D suitCollider;              
    [SerializeField] private PhysicsMaterial2D frictionMaterial;   
    [SerializeField] private PhysicsMaterial2D noFrictionMaterial; 

    private PhysicsMaterial2D originalMaterial;

    private GameObject ratInRange;

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (suitCollider == null) suitCollider = GetComponent<Collider2D>();
        if (suitCollider != null) originalMaterial = suitCollider.sharedMaterial;
        ApplyState(state);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        bool canShowPrompt =
            state == SuitState.Empty &&
            ratInRange != null &&
            Time.time >= blockEnterUntil;

        if (promptUI != null)
            promptUI.SetActive(canShowPrompt);

        if (Input.GetKeyDown(interactKey))
        {
            if (state == SuitState.Empty && ratInRange != null && Time.time >= blockEnterUntil)
            {
                EnterSuit(ratInRange);
            }
            else if (state == SuitState.Occupied)
            {
                ExitSuit();
            }
        }
    }

    void EnterSuit(GameObject rat)
    {
        rat.SetActive(false);
        ApplySuitMaterial(true);

        ratInRange = null;
        if (promptUI != null) promptUI.SetActive(false);

        PlayEnterSound();

        state = SuitState.Occupied;
        ApplyState(state);

        if (cameraFollow != null)
            cameraFollow.SetTarget(suitCameraTarget != null ? suitCameraTarget : transform);

    }

    void ExitSuit()
    {
        PlayExitSound();
        
        blockEnterUntil = Time.time + reenterBlockTime;

        var rat = FindFirstObjectByType<RatController>(FindObjectsInactive.Include);
        if (rat != null)
        {
            var ratGO = rat.gameObject;

            ratGO.transform.position = ejectPoint != null ? ejectPoint.position : transform.position;
            ratGO.SetActive(true);

            var rb = ratGO.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float dir = transform.localScale.x >= 0 ? 1f : -1f; 
                rb.linearVelocity = new Vector2(dir * ejectForceX, ejectForceY);
            }
        }

        state = SuitState.Empty;
        ApplyState(state);
        if (cameraFollow != null && ratCameraTarget != null)
            cameraFollow.SetTarget(ratCameraTarget);

        ApplySuitMaterial(false);

    }

    void ApplyState(SuitState s)
    {
        bool occupied = s == SuitState.Occupied;

        if (anim != null)
            anim.SetBool("Occupied", occupied);

        if (suitMovementScript != null)
            suitMovementScript.enabled = occupied; 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != SuitState.Empty) return;
        if (other.CompareTag("Rat"))
            ratInRange = other.gameObject;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (ratInRange == null) return;
        if (other.gameObject == ratInRange)
            ratInRange = null;
    }

    void PlayEnterSound()
    {
        if (enterSuitClips == null || enterSuitClips.Length == 0) return;
        if (audioSource == null) return;

        AudioClip clip = enterSuitClips[Random.Range(0, enterSuitClips.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }

    void PlayExitSound()
    {
        if (exitSuitClips == null || exitSuitClips.Length == 0) return;
        if (audioSource == null) return;

        AudioClip clip = exitSuitClips[Random.Range(0, exitSuitClips.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }
    private void ApplySuitMaterial(bool occupied)
    {
        if (suitCollider == null) return;

        
        suitCollider.sharedMaterial = occupied ? noFrictionMaterial : frictionMaterial;

       
        var rb = suitCollider.attachedRigidbody;
        if (rb != null)
        {
            rb.sharedMaterial = null; 
        }
    }
}
