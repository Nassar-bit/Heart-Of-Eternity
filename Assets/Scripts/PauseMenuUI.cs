using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pausePanel;
    public GameObject pauseButtonObject;

    [Header("Main Menu Scene")]
    public string mainMenuSceneName = "SampleScene";

    [Header("Gameplay Scripts To Disable While Paused")]
    public Behaviour[] gameplayScriptsToDisable;

    [Header("Hotkeys")]
    public KeyCode pauseKey = KeyCode.P;
    public bool allowResumeWithKey = true;

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SetCursorForGameplay();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButtonObject != null)
            pauseButtonObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else if (allowResumeWithKey)
            {
                ResumeGame();
            }
        }

        // Editor mein aksar cursor state reset ho jata hai, isliye ensure rakho
        if (isPaused)
        {
            SetCursorForUI();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        SetCursorForUI();
        SetGameplayScripts(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseButtonObject != null)
            pauseButtonObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        SetCursorForGameplay();
        SetGameplayScripts(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButtonObject != null)
            pauseButtonObject.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetCursorForUI();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetCursorForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetGameplayScripts(bool isEnabled)
    {
        if (gameplayScriptsToDisable == null) return;

        for (int i = 0; i < gameplayScriptsToDisable.Length; i++)
        {
            if (gameplayScriptsToDisable[i] != null)
                gameplayScriptsToDisable[i].enabled = isEnabled;
        }
    }
}