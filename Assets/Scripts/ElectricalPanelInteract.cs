using UnityEngine;

public class ElectricalPanelInteract : MonoBehaviour
{
    [Header("Interact")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private DoorUp doorToOpen;
    [SerializeField] private bool oneUse = true;

    [Header("Prompt")]
    [SerializeField] private GameObject promptE;

    [Header("Audio")]
    [SerializeField] private AudioClip interactClip;      
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Detection")]
    [SerializeField] private string ratTag = "Rat";

    private AudioSource audioSource;
    private bool ratInRange;
    private bool used;

    private void Awake()
    {
        if (promptE != null)
            promptE.SetActive(false);

        // Creamos / agarramos AudioSource 
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (!ratInRange) return;
        if (used && oneUse) return;

        if (Input.GetKeyDown(interactKey))
        {
            PlaySound();
            doorToOpen?.Open();
            used = true;

            if (promptE != null)
                promptE.SetActive(false);
        }
    }

    private void PlaySound()
    {
        if (interactClip == null) return;

        audioSource.clip = interactClip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.Stop();
        audioSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(ratTag)) return;

        ratInRange = true;

        if (!used && promptE != null)
            promptE.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(ratTag)) return;

        ratInRange = false;

        if (promptE != null)
            promptE.SetActive(false);
    }
}
