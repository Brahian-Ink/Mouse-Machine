using UnityEngine;

public class OverheatBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private float maxWidth = 200f;

    private SuitOverheat overheat;

    private void Awake()
    {
        gameObject.SetActive(true);
        Debug.Log("[OverheatBarUI] Awake OK - UI visible");
    }

    private void Update()
    {
        if (overheat == null || fillRect == null) return;

        float v = Mathf.Clamp01(overheat.Heat01);
        var size = fillRect.sizeDelta;
        size.x = maxWidth * v;
        fillRect.sizeDelta = size;
    }

    public void Bind(SuitOverheat newOverheat)
    {
        overheat = newOverheat;
        Debug.Log("Bind OK");
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        Debug.Log("SetVisible: " + visible);
    }
}
