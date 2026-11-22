using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.6f;
    public float gravity = -20f;
    public float groundedGraceTime = 0.15f;

    [Header("Mouse Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float groundedTimer = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // Mouse Look
        if (playerCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        // Grounded Grace Timer
        if (controller.isGrounded)
        {
            groundedTimer = groundedGraceTime;
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }

        // Horizontal Movement (No Sprint)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 horizontalMove = (transform.right * x + transform.forward * z) * moveSpeed;

        // Jump (Spacebar)
        if (Input.GetButtonDown("Jump") && groundedTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            groundedTimer = 0f;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        // Single Combined Move
        Vector3 finalMove = horizontalMove + new Vector3(0f, velocity.y, 0f);
        controller.Move(finalMove * Time.deltaTime);
    }
}