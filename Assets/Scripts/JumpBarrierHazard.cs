using UnityEngine;

public class JumpBarrierHazard : MonoBehaviour
{
    [Header("Optional Movement")]
    public bool moveBarrier = false;
    public Vector3 moveDirection = Vector3.right;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    [Header("Respawn Settings")]
    public string playerTag = "Player";
    public Transform respawnPoint;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!moveBarrier) return;

        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + moveDirection.normalized * offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (respawnPoint != null)
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            other.transform.position = respawnPoint.position;

            if (cc != null) cc.enabled = true;
        }
    }
}