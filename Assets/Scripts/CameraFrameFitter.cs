using UnityEngine;

[ExecuteAlways]
public class CameraFrameFitter : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Frame Parts")]
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private Transform left;
    [SerializeField] private Transform right;

    [Header("Frame Thickness (world units)")]
    [SerializeField] private float thickness = 0.5f;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        Fit();
    }

    private void LateUpdate()
    {
        Fit();
    }

    private void Fit()
    {
        if (cam == null || !cam.orthographic) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;

        
        if (top)
        {
            top.position = new Vector3(camPos.x, camPos.y + camHeight / 2f + thickness / 2f, 0);
            top.localScale = new Vector3(camWidth + thickness * 2f, thickness, 1);
        }

        if (bottom)
        {
            bottom.position = new Vector3(camPos.x, camPos.y - camHeight / 2f - thickness / 2f, 0);
            bottom.localScale = new Vector3(camWidth + thickness * 2f, thickness, 1);
        }

        if (left)
        {
            left.position = new Vector3(camPos.x - camWidth / 2f - thickness / 2f, camPos.y, 0);
            left.localScale = new Vector3(thickness, camHeight, 1);
        }

        if (right)
        {
            right.position = new Vector3(camPos.x + camWidth / 2f + thickness / 2f, camPos.y, 0);
            right.localScale = new Vector3(thickness, camHeight, 1);
        }
    }
}
