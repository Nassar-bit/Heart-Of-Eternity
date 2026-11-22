using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gameplay Progression")]
    public int totalKeysRequired = 2;
    public int pointsPerKey = 100;
    public int heartBonusPoints = 250;

    [Header("UI References")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI interactPromptText;
    public GameObject victoryPanel;
    public GameObject gameOverPanel; // Optional

    private int keysCollected = 0;
    private int score = 0;
    private bool hasHeartOfEternity = false;
    private bool isGameEnded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (interactPromptText != null)
        {
            interactPromptText.text = "";
            interactPromptText.gameObject.SetActive(false);
        }

        UpdateUI();
    }

    public void CollectKey(int bonusPoints = 0)
    {
        if (isGameEnded) return;

        keysCollected++;
        score += pointsPerKey + bonusPoints;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (isGameEnded) return;
        score += amount;
        UpdateUI();
    }

    public bool HasKey(int keyNumber = 1)
    {
        return keysCollected >= keyNumber;
    }

    public bool HasAllKeys()
    {
        return keysCollected >= totalKeysRequired;
    }

    public void ClaimHeartOfEternity()
    {
        if (isGameEnded) return;

        hasHeartOfEternity = true;
        score += heartBonusPoints;
        isGameEnded = true;

        UpdateUI();
        ShowInteractPrompt("");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void TriggerGameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        ShowInteractPrompt("");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowInteractPrompt(string message)
    {
        if (interactPromptText != null)
        {
            bool shouldShow = !isGameEnded && !string.IsNullOrEmpty(message);
            interactPromptText.gameObject.SetActive(shouldShow);
            interactPromptText.text = shouldShow ? message : "";
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score} | Keys: {keysCollected}/{totalKeysRequired}";
        }

        if (objectiveText != null)
        {
            if (hasHeartOfEternity)
            {
                objectiveText.text = "Objective:\nHeart of Eternity Claimed!";
            }
            else if (keysCollected < totalKeysRequired)
            {
                int nextKey = keysCollected + 1;
                objectiveText.text = $"Objective:\nExplore the halls & find Key {nextKey}";
            }
            else
            {
                objectiveText.text = "Objective:\nUnlock the door & claim the Heart of Eternity!";
            }
        }
    }
}