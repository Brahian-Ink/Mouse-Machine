using UnityEngine;

public class SuitOverheat : MonoBehaviour
{
    [Header("Overheat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatPerShot = 12f;
    [SerializeField] private float coolPerSecond = 25f;

    [Header("Lock")]
    [SerializeField] private float unlockHeatThreshold = 60f; 

    public float Heat { get; private set; }
    public float Heat01 => Mathf.Clamp01(Heat / maxHeat);
    public bool IsOverheated { get; private set; }

    public bool CanShoot => !IsOverheated;

    private void Update()
    {
        if (Heat > 0f)
        {
            Heat = Mathf.Max(0f, Heat - coolPerSecond * Time.deltaTime);
        }

        if (IsOverheated && Heat <= unlockHeatThreshold)
        {
            IsOverheated = false;
        }
    }

    public void AddHeatForShot()
    {
        if (IsOverheated) return;

        Heat = Mathf.Min(maxHeat, Heat + heatPerShot);

        if (Heat >= maxHeat)
            IsOverheated = true;
    }

    public void ResetHeat()
    {
        Heat = 0f;
        IsOverheated = false;
    }
}
