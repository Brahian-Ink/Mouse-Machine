using UnityEngine;

public class JumpSfx : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] jumpClips;

    [Header("Tuning")]
    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new(0.8f, 1f);

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayJump()
    {
        if (jumpClips == null || jumpClips.Length == 0) return;
        if (source == null) return;

        var clip = jumpClips[Random.Range(0, jumpClips.Length)];
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }
}
