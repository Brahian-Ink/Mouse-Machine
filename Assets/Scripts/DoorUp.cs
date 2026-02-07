using UnityEngine;
using System.Collections;

public class DoorUp : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private Transform door;           
    [SerializeField] private float openHeight = 2.5f;  // cuánto sube en unidades
    [SerializeField] private float openTime = 0.35f;   // tiempo de animación



    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen;
    private bool isMoving;

    private void Awake()
    {
        if (door == null) door = transform;
        closedPos = door.position;
        openPos = closedPos + Vector3.up * openHeight;
    }

    public void Open()
    {
        if (isOpen || isMoving) return;
        StartCoroutine(MoveDoor(closedPos, openPos, true));
    }

    public void Close()
    {
        if (!isOpen || isMoving) return;
        StartCoroutine(MoveDoor(openPos, closedPos, false));
    }

    private IEnumerator MoveDoor(Vector3 from, Vector3 to, bool opening)
    {
        isMoving = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openTime);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            door.position = Vector3.Lerp(from, to, eased);
            yield return null;
        }

        door.position = to;
        isOpen = opening;
        isMoving = false;

       
    }
}
