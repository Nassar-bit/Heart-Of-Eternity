using UnityEngine;

public class HingeDoor : MonoBehaviour
{
    [Header("Swing Settings")]
    public float openAngle = 90f;
    public float openSpeed = 5f;
    public bool forceCurrentAsClosedOnStart = true;

    [Header("Optional Collider To Disable When Open")]
    public Collider blockingCollider;

    private Quaternion closedLocalRot;
    private Quaternion targetLocalRot;
    private bool isOpen = false;

    private void Awake()
    {
        closedLocalRot = transform.localRotation;
        targetLocalRot = closedLocalRot;
    }

    private void Start()
    {
        if (forceCurrentAsClosedOnStart && !isOpen)
        {
            transform.localRotation = closedLocalRot;
            targetLocalRot = closedLocalRot;
        }
    }

    private void LateUpdate()
    {
        if (!isOpen && forceCurrentAsClosedOnStart)
        {
            transform.localRotation = closedLocalRot;
            return;
        }

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetLocalRot,
            Time.deltaTime * openSpeed
        );
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        targetLocalRot = closedLocalRot * Quaternion.Euler(0f, openAngle, 0f);
        Debug.Log($"[HingeDoor] OpenDoor() triggered on {gameObject.name}");

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }
    }

    public void CloseDoor()
    {
        isOpen = false;
        targetLocalRot = closedLocalRot;
        transform.localRotation = closedLocalRot;

        if (blockingCollider != null)
        {
            blockingCollider.enabled = true;
        }
    }
}