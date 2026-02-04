using UnityEngine;

public class LandingSfx : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] landClips;

    [Header("Tuning")]
    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new(0.9f, 1.1f);

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayLand(float impact)
    {
        if (landClips == null || landClips.Length == 0) return;
        if (source == null) return;

        // Impact normalizado (0–1)
        float vol = Mathf.Lerp(volumeRange.x, volumeRange.y, impact);
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);

        var clip = landClips[Random.Range(0, landClips.Length)];
        source.PlayOneShot(clip, vol);
    }
}
