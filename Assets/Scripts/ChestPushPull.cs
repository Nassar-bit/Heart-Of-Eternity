using UnityEngine;

public class ChestPushPull : MonoBehaviour
{
    public Camera playerCamera;
    public float reachDistance = 5f;
    public float pushForce = 6f;
    public float dragFollowSpeed = 8f;
    public KeyCode dragKey = KeyCode.F;
    public string dragPrompt = "Press [F] to drag";
    public string dropPrompt = "Press [F] to drop";

    private Rigidbody heldChestRb;
    private float holdDistance;
    private bool showingDragPrompt = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerCamera == null) return;

        if (Input.GetKeyDown(dragKey))
        {
            if (heldChestRb != null)
            {
                ReleaseChest();
            }
            else
            {
                TryGrabChest();
            }
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerCamera == null) return;

        UpdatePrompt();
    }

    void FixedUpdate()
    {
        if (heldChestRb != null && playerCamera != null)
        {
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
            targetPos.y = heldChestRb.position.y;

            Vector3 toTarget = targetPos - heldChestRb.position;
            heldChestRb.velocity = toTarget * dragFollowSpeed;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (heldChestRb != null) return;
        if (!hit.collider.CompareTag("DraggableChest")) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }

    void TryGrabChest()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (hit.collider.CompareTag("DraggableChest") && rb != null && !rb.isKinematic)
            {
                heldChestRb = rb;
                holdDistance = Mathf.Clamp(hit.distance, 1.8f, reachDistance);
            }
        }
    }

    void ReleaseChest()
    {
        if (heldChestRb != null)
        {
            heldChestRb.velocity = Vector3.zero;
            heldChestRb = null;
        }
    }

    void UpdatePrompt()
    {
        if (GameManager.Instance == null) return;

        if (heldChestRb != null)
        {
            GameManager.Instance.ShowInteractPrompt(dropPrompt);
            showingDragPrompt = true;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance) && hit.collider.CompareTag("DraggableChest"))
        {
            GameManager.Instance.ShowInteractPrompt(dragPrompt);
            showingDragPrompt = true;
        }
        else if (showingDragPrompt)
        {
            GameManager.Instance.ShowInteractPrompt("");
            showingDragPrompt = false;
        }
    }
}