using System.Collections;
using UnityEngine;

public class SquidBlockTile : MonoBehaviour
{
    [Header("Block Setup")]
    public bool isSafeTile = false;
    public string playerTag = "Player";
    public Transform respawnPoint;
    public float fallSpeed = 12f;

    [Header("Landing Check")]
    public bool useSurfaceCheck = true;
    public float topCheckHeight = 0.14f;
    [Range(0.2f, 0.9f)]
    public float surfaceWidthFactor = 0.6f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stepClip;   // Jaise hi tile par charhay tab bajega
    public AudioClip breakClip;  // Trap tile tootne par bajega
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    public float stepCooldown = 0.25f;
    public bool playAs2D = true;

    private bool triggered = false;
    private bool stepSoundPlayed = false;
    private float lastStepTime = -10f;
    private Collider tileCollider;

    private void Awake()
    {
        tileCollider = GetComponent<Collider>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (triggered || !useSurfaceCheck) return;
        CheckPlayerStandingOnTop();
    }

    private void CheckPlayerStandingOnTop()
    {
        if (tileCollider == null) return;

        Bounds b = tileCollider.bounds;
        float halfH = Mathf.Max(0.02f, topCheckHeight * 0.5f);
        Vector3 center = new Vector3(b.center.x, b.max.y + halfH, b.center.z);
        Vector3 halfExtents = new Vector3(
            Mathf.Max(0.05f, b.extents.x * surfaceWidthFactor),
            halfH,
            Mathf.Max(0.05f, b.extents.z * surfaceWidthFactor)
        );

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == tileCollider) continue;

            Transform player = FindPlayerTransform(hits[i]);
            if (player != null)
            {
                if (player.position.y > b.max.y + 1.2f) continue;

                OnPlayerSteppedOnTile(player);
                return;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;

        bool hitFromAbove = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < -0.5f)
            {
                hitFromAbove = true;
                break;
            }
        }
        if (!hitFromAbove) return;

        Transform player = FindPlayerTransform(collision.collider);
        if (player != null) OnPlayerSteppedOnTile(player);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (triggered) return;
        if (hit.normal.y < 0.5f) return;

        Transform player = FindPlayerTransform(hit.collider);
        if (player != null) OnPlayerSteppedOnTile(player);
    }

    private void OnPlayerSteppedOnTile(Transform playerTransform)
    {
        if (triggered || playerTransform == null) return;

        // Jaise hi tile par charhay, foran stepClip bajao
        PlayStepSound(playerTransform);

        if (!isSafeTile)
        {
            TriggerTrap(playerTransform);
        }
    }

    private void PlayStepSound(Transform playerTransform)
    {
        if (stepClip == null || stepSoundPlayed) return;
        if (Time.unscaledTime - lastStepTime < stepCooldown) return;

        stepSoundPlayed = true;
        lastStepTime = Time.unscaledTime;
        PlayClipNow(stepClip, playerTransform);
    }

    private void TriggerTrap(Transform playerTransform)
    {
        if (triggered || isSafeTile || playerTransform == null) return;
        triggered = true;

        // Agar breakClip alag se diya hai to woh foran bajao
        if (breakClip != null)
        {
            PlayClipNow(breakClip, playerTransform);
        }

        if (tileCollider != null) tileCollider.enabled = false;

        PlayerLives lives = playerTransform.GetComponentInParent<PlayerLives>();
        if (lives == null)
        {
            lives = FindObjectOfType<PlayerLives>();
        }

        if (lives != null)
        {
            lives.LoseLife(teleportToSafePoint: false);

            if (lives.currentLives > 0 && respawnPoint != null)
            {
                TeleportPlayer(playerTransform, respawnPoint.position);
            }
        }

        StartCoroutine(DropTile());
    }

    private void PlayClipNow(AudioClip clip, Transform playerTransform)
    {
        if (clip == null) return;

        if (playAs2D)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : playerTransform.position;
            AudioSource.PlayClipAtPoint(clip, soundPos, sfxVolume);
            return;
        }

        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.mute = false;
            audioSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume);
        }
    }

    private Transform FindPlayerTransform(Collider hitCollider)
    {
        if (hitCollider == null) return null;

        if (hitCollider.CompareTag(playerTag))
            return hitCollider.transform;

        if (hitCollider.transform.root != null && hitCollider.transform.root.CompareTag(playerTag))
            return hitCollider.transform.root;

        PlayerLives lives = hitCollider.GetComponentInParent<PlayerLives>();
        if (lives != null)
            return lives.transform;

        return null;
    }

    private void TeleportPlayer(Transform player, Vector3 targetPos)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = targetPos;

        if (cc != null) cc.enabled = true;
    }

    private IEnumerator DropTile()
    {
        float t = 0f;
        while (t < 1.5f)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}