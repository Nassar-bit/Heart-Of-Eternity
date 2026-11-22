using UnityEngine;
using TMPro;

public class RiddleForKey2 : MonoBehaviour
{
    [Header("Riddle Setup")]
    [TextArea(2, 5)]
    public string riddleQuestion = "I have halls without rooms, walls without end, and yellow lights that never mend. What am I?";
    public string correctAnswer = "Backrooms";
    public string wrongAnswerMessage = "Wrong answer! Try again.";
    public string solvedPrompt = "Riddle solved! Key 2 appeared.";

    [Header("Key 2 Object")]
    public GameObject key2Object; // Shuru mein Hierarchy mein unchecked rakho
    public bool giveKeyDirectly = false;

    [Header("UI References")]
    public GameObject riddlePanel;
    public TextMeshProUGUI riddleText;
    public TMP_InputField answerInput;
    public TextMeshProUGUI feedbackText;

    private bool isSolved = false;
    private bool isOpen = false;

    void Start()
    {
        if (riddlePanel != null) riddlePanel.SetActive(false);
        if (key2Object != null && !giveKeyDirectly) key2Object.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseRiddle();
        }
    }

    public string GetPrompt()
    {
        if (isSolved) return solvedPrompt;
        return "Press [E] to answer riddle";
    }

    public void OpenRiddle()
    {
        if (isSolved || isOpen || riddlePanel == null) return;

        isOpen = true;
        riddlePanel.SetActive(true);

        if (riddleText != null) riddleText.text = riddleQuestion;
        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
        }
        if (feedbackText != null) feedbackText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void CloseRiddle()
    {
        isOpen = false;
        if (riddlePanel != null) riddlePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void SubmitAnswer()
    {
        if (isSolved || answerInput == null) return;

        string playerAnswer = answerInput.text.Trim();
        if (string.Equals(playerAnswer, correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            isSolved = true;
            CloseRiddle();

            if (giveKeyDirectly)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.CollectKey();
            }
            else if (key2Object != null)
            {
                key2Object.SetActive(true);
            }
        }
        else
        {
            if (feedbackText != null)
                feedbackText.text = wrongAnswerMessage;
        }
    }
}