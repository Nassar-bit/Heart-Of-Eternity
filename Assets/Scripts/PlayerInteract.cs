using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3.5f;
    public float sphereRadius = 0.25f;
    public TextMeshProUGUI promptText;
    public KeyCode interactKey = KeyCode.E;

    private InteractableItem currentItem;

    void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        CheckForInteractable();

        if (currentItem != null && Input.GetKeyDown(interactKey))
        {
            string resultMessage = currentItem.Interact();
            if (!string.IsNullOrEmpty(resultMessage) && promptText != null)
            {
                promptText.gameObject.SetActive(true);
                promptText.text = resultMessage;
            }
        }
    }

    void CheckForInteractable()
    {
        currentItem = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactDistance))
        {
            InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
            if (item != null)
            {
                currentItem = item;
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = item.GetPrompt();
                }
                return;
            }
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}