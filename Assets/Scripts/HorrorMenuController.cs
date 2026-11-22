using UnityEngine;

public class HorrorMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;

    [Header("Menu Audio")]
    public AudioSource menuAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;
    [Range(0f, 1f)]
    public float menuSfxVolume = 0.8f;

    [Header("Gameplay Scripts To Disable In Menu / Pause")]
    public Behaviour[] gameplayScriptsToDisable;

    private bool isInStartMenu = true;
    private bool isPaused = false;

    private void Awake()
    {
        if (menuAudioSource == null)
        {
            menuAudioSource = GetComponent<AudioSource>();
        }

        OpenStartMenu();
    }

    private void Update()
    {
        if (isInStartMenu) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PlayHoverSound()
    {
        if (menuAudioSource != null && hoverClip != null)
        {
            menuAudioSource.PlayOneShot(hoverClip, menuSfxVolume);
        }
    }

    public void PlayClickSound()
    {
        if (menuAudioSource != null && clickClip != null)
        {
            menuAudioSource.PlayOneShot(clickClip, menuSfxVolume);
        }
    }

    public void OpenStartMenu()
    {
        isInStartMenu = true;
        isPaused = false;

        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        SetGameplayLocked(true);
    }

    public void StartGame()
    {
        PlayClickSound();
        isInStartMenu = false;
        isPaused = false;

        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        SetGameplayLocked(false);
    }

    public void PauseGame()
    {
        if (isInStartMenu) return;
        isPaused = true;

        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        SetGameplayLocked(true);
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        SetGameplayLocked(false);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetGameplayLocked(bool locked)
    {
        if (gameplayScriptsToDisable != null)
        {
            for (int i = 0; i < gameplayScriptsToDisable.Length; i++)
            {
                if (gameplayScriptsToDisable[i] != null)
                {
                    gameplayScriptsToDisable[i].enabled = !locked;
                }
            }
        }

        Time.timeScale = locked ? 0f : 1f;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}