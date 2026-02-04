using UnityEngine;

public class RatFootstepsSfx : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] stepClips;

    [Header("Tuning")]
    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.1f);
    [SerializeField] private Vector2 volumeRange = new(0.6f, 0.9f);
    [SerializeField] private float minSpeedToPlay = 0.1f;

    // seteado por RatController
    public float CurrentSpeed01 { get; set; }
    public bool IsGrounded { get; set; }

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    // Llamado desde Animation Event
    public void PlayStep()
    {
        if (!IsGrounded) return;
        if (CurrentSpeed01 < minSpeedToPlay) return;
        if (stepClips == null || stepClips.Length == 0) return;

        var clip = stepClips[Random.Range(0, stepClips.Length)];
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }
}
