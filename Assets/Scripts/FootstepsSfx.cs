using UnityEngine;

public class FootstepsSfx : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] stepClips;

    [Header("Audio")]
    [SerializeField] private AudioSource source;

    [Header("Tuning")]
    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new(0.7f, 1f);

    [Header("Gate")]
    [SerializeField] private float minSpeedToPlay = 0.1f;

    public float CurrentSpeed01 { get; set; }

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayStep()
    {
        if (CurrentSpeed01 < minSpeedToPlay) return;
        if (stepClips == null || stepClips.Length == 0) return;

        AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }
}
