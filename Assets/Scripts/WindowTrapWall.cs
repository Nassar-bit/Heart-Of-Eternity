using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowTrapWall : MonoBehaviour
{
    [Header("Trap Wall & Movement")]
    public Transform trapWall;
    public bool useLocalDirection = true;
    public Vector3 moveDirection = Vector3.forward;
    public float totalTravelDistance = 8f;
    public float slowSpeed = 1.5f;
    [Range(0.1f, 0.9f)]
    public float pauseAtFraction = 0.65f;
    public float pauseBeforeSlam = 0.8f;
    public float slamSpeed = 25f;

    [Header("Impact & Start Respawn")]
    public string playerTag = "Player";
    public Transform playerCamera;
    public Transform startSpawnPoint; // Agar khali chhoro ge to game start wali player position khud save ho jayegi
    public float delayBeforeWallStarts = 1.0f;
    public float impactFlashDuration = 0.6f;
    public float impactFlashAlpha = 0.72f;
    public float cameraShakeMagnitude = 0.25f;
    public bool deductLifeOnImpact = true;
    public bool restartSceneWhenOutOfLives = true;
    public float zeroLivesRestartDelay = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip creepClip;
    public AudioClip slamClip;
    [Range(0f, 1f)]
    public float audioVolume = 1f;
    public bool use2DAudioForTest = true;

    private bool triggered = false;
    private bool showImpactFlash = false;
    private Transform hitPlayer;
    private PlayerLives hitPlayerLives;
    private Texture2D redTexture;
    private Vector3 initialWallPos;
    private Quaternion initialWallRot;
    private Vector3 initialPlayerPos;
    private Quaternion initialPlayerRot;
    private bool hasSavedPlayerStart = false;

    private void Awake()
    {
        redTexture = new Texture2D(1, 1);
        redTexture.SetPixel(0, 0, new Color(0.65f, 0.02f, 0.04f, 1f));
        redTexture.Apply();

        if (trapWall != null)
        {
            initialWallPos = trapWall.position;
            initialWallRot = trapWall.rotation;
        }
    }

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        SaveInitialPlayerStartIfNeeded();
    }

    private void SaveInitialPlayerStartIfNeeded()
    {
        if (hasSavedPlayerStart) return;

        if (startSpawnPoint != null)
        {
            initialPlayerPos = startSpawnPoint.position;
            initialPlayerRot = startSpawnPoint.rotation;
            hasSavedPlayerStart = true;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            initialPlayerPos = playerObj.transform.position;
            initialPlayerRot = playerObj.transform.rotation;
            hasSavedPlayerStart = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        Transform p = FindPlayer(other);
        if (p == null) return;

        triggered = true;
        hitPlayer = p;
        hitPlayerLives = p.GetComponentInParent<PlayerLives>();

        if (!hasSavedPlayerStart)
        {
            initialPlayerPos = hitPlayer.position;
            initialPlayerRot = hitPlayer.rotation;
            hasSavedPlayerStart = true;
        }

        StartCoroutine(RunTrapSequence());
    }

    private Transform FindPlayer(Collider col)
    {
        if (col == null) return null;
        if (col.CompareTag(playerTag)) return col.transform;
        if (col.transform.root != null && col.transform.root.CompareTag(playerTag)) return col.transform.root;
        return null;
    }

    private IEnumerator RunTrapSequence()
    {
        if (delayBeforeWallStarts > 0f)
        {
            yield return new WaitForSeconds(delayBeforeWallStarts);
        }

        PlaySound(creepClip, loop: true);

        if (trapWall != null)
        {
            Vector3 dir = useLocalDirection
                ? trapWall.TransformDirection(moveDirection).normalized
                : moveDirection.normalized;

            float movedSoFar = 0f;
            float pauseTarget = totalTravelDistance * Mathf.Clamp01(pauseAtFraction);

            while (movedSoFar < pauseTarget)
            {
                float step = Mathf.Min(slowSpeed * Time.deltaTime, pauseTarget - movedSoFar);
                trapWall.position += dir * step;
                movedSoFar += step;
                yield return null;
            }

            StopSound();
            if (pauseBeforeSlam > 0f)
            {
                yield return new WaitForSeconds(pauseBeforeSlam);
            }

            while (movedSoFar < totalTravelDistance)
            {
                float step = Mathf.Min(slamSpeed * Time.deltaTime, totalTravelDistance - movedSoFar);
                trapWall.position += dir * step;
                movedSoFar += step;
                yield return null;
            }
        }
        else
        {
            StopSound();
        }

        PlaySound(slamClip, loop: false);
        yield return StartCoroutine(HandleImpactAndSendToStart());
    }

    private void PlaySound(AudioClip clip, bool loop)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = audioVolume;
        audioSource.spatialBlend = use2DAudioForTest ? 0f : 1f;
        audioSource.mute = false;
        audioSource.Play();
    }

    private void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    private IEnumerator HandleImpactAndSendToStart()
    {
        showImpactFlash = true;

        PlayerLives lives = hitPlayerLives != null ? hitPlayerLives : FindObjectOfType<PlayerLives>();
        if (deductLifeOnImpact && lives != null)
        {
            lives.LoseLife(teleportToSafePoint: false);
        }

        float elapsed = 0f;
        Vector3 originalCamLocalPos = playerCamera != null ? playerCamera.localPosition : Vector3.zero;

        while (elapsed < impactFlashDuration)
        {
            if (playerCamera != null && cameraShakeMagnitude > 0f)
            {
                playerCamera.localPosition = originalCamLocalPos + (Random.insideUnitSphere * cameraShakeMagnitude);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (playerCamera != null)
        {
            playerCamera.localPosition = originalCamLocalPos;
        }

        showImpactFlash = false;
        StopSound();

        // Agar lives 0 ho gayi hain to scene restart
        if (restartSceneWhenOutOfLives && lives != null && lives.currentLives <= 0)
        {
            yield return new WaitForSecondsRealtime(zeroLivesRestartDelay);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        // Player ko start wali jagah wapas khara karna
        TeleportPlayerToStart();

        // Wall ko wapas shuru wali jagah set karna
        if (trapWall != null)
        {
            trapWall.position = initialWallPos;
            trapWall.rotation = initialWallRot;
        }

        triggered = false;
    }

    private void TeleportPlayerToStart()
    {
        Transform player = hitPlayer;
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag(playerTag);
            if (pObj != null) player = pObj.transform;
        }

        if (player == null || !hasSavedPlayerStart) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = initialPlayerPos;
        player.rotation = initialPlayerRot;

        if (cc != null) cc.enabled = true;
    }

    private void OnGUI()
    {
        if (!showImpactFlash || redTexture == null) return;

        Color prevColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, impactFlashAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), redTexture);
        GUI.color = prevColor;
    }
}