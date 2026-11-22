using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLives : MonoBehaviour
{
    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;
    public TextMeshProUGUI livesText;

    [Header("Anti-Double-Hit")]
    public float hitCooldown = 1.2f;
    private float lastHitTime = -10f;

    [Header("Respawn")]
    public Transform currentSafePoint;
    public float respawnDelay = 0.25f;

    [Header("Death / Restart Flash")]
    public float gameOverFlashAlpha = 0.75f;
    public float restartDelay = 0.9f;

    private bool isDead = false;
    private bool showDeathFlash = false;
    private Texture2D redTex;
    private CharacterController charController;
    private Rigidbody rb;

    private void Awake()
    {
        currentLives = maxLives;
        charController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (currentSafePoint == null)
        {
            GameObject p = new GameObject("InitialSafePoint");
            p.transform.position = transform.position;
            currentSafePoint = p.transform;
        }

        redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, new Color(0.65f, 0.02f, 0.04f, 1f));
        redTex.Apply();

        UpdateLivesUI();
    }

    public void SetSafePoint(Transform newPoint)
    {
        if (newPoint != null)
        {
            currentSafePoint = newPoint;
        }
    }

    public void LoseLife(bool teleportToSafePoint = true)
    {
        if (isDead) return;

        if (Time.unscaledTime - lastHitTime < hitCooldown)
        {
            Debug.Log("[PlayerLives] Ignored duplicate hit during cooldown.");
            return;
        }

        lastHitTime = Time.unscaledTime;

        currentLives--;
        if (currentLives < 0) currentLives = 0;

        UpdateLivesUI();
        Debug.Log($"[PlayerLives] Life lost! Remaining lives: {currentLives}/{maxLives}");

        if (currentLives <= 0)
        {
            StartCoroutine(HandleGameOver());
            return;
        }

        if (teleportToSafePoint && currentSafePoint != null)
        {
            StartCoroutine(RespawnAtSafePoint());
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
    }

    private IEnumerator RespawnAtSafePoint()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (charController != null) charController.enabled = false;
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = currentSafePoint.position;

        if (charController != null) charController.enabled = true;
    }

    private IEnumerator HandleGameOver()
    {
        if (isDead) yield break;
        isDead = true;
        showDeathFlash = true;

        Debug.Log("[PlayerLives] All lives lost! Restarting scene...");

        yield return new WaitForSecondsRealtime(restartDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        if (!showDeathFlash || redTex == null) return;

        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, gameOverFlashAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), redTex);
        GUI.color = oldColor;
    }
}