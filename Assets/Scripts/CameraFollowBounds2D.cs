using UnityEngine;

[ExecuteAlways]
public class CameraFollowBounds2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField, Range(0f, 30f)] private float smooth = 12f;
    [Tooltip("Si querés que el follow sea instantáneo (tipo lock).")]
    [SerializeField] private bool instant = false;


    [Header("Pixel Snap")]
    [SerializeField] private bool pixelSnap = false;
    [Tooltip("Pixels Per Unit del sprite (ej: 16, 32, 64).")]
    [SerializeField] private int pixelsPerUnit = 16;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || cam == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        Vector3 next = desired;

        if (!instant)
        {
            float t = 1f - Mathf.Exp(-smooth * Time.deltaTime); 
            next = Vector3.Lerp(transform.position, desired, t);
        }

    

        if (pixelSnap && pixelsPerUnit > 0)
        {
            float unitsPerPixel = 1f / pixelsPerUnit;
            next.x = Mathf.Round(next.x / unitsPerPixel) * unitsPerPixel;
            next.y = Mathf.Round(next.y / unitsPerPixel) * unitsPerPixel;
        }

        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}
