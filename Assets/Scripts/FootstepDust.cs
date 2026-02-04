using UnityEngine;

public class FootstepDust : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private Transform dustSpawnPoint;

    [Header("Timing")]
    [SerializeField] private float stepInterval = 0.25f;

    float nextStepTime;

    // Seteado desde el controller
    public float Speed01 { get; set; }
    public bool Grounded { get; set; }

    void Update()
    {
        if (!Grounded) return;
        if (Speed01 < 0.1f) return;
        if (Time.time < nextStepTime) return;

        SpawnDust();
        nextStepTime = Time.time + stepInterval;
    }

    void SpawnDust()
    {
        if (dustPrefab == null || dustSpawnPoint == null) return;
        Instantiate(dustPrefab, dustSpawnPoint.position, Quaternion.identity);
    }
}
