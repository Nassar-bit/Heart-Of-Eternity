using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpScene : MonoBehaviour
{
    [Header("Target Scene")]
    public string targetSceneName = "LevelTst";

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip clickClip;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void JumpToScene()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip, sfxVolume);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }

    public void JumpToSpecificScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip, sfxVolume);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip, sfxVolume);
        }

        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}