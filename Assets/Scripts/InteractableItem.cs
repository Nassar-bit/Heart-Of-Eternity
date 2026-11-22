using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public enum ItemType
    {
        Key,
        LockedDoorOrChest
    }

    [Header("Item Settings")]
    public ItemType itemType = ItemType.Key;
    public string itemLabel = "Key 2";
    public string interactPrompt = "Press [E] to collect";

    [Header("Key / Lock Settings")]
    public int requiredKeyNumber = 2;
    public string lockedMessage = "Locked! Find Key 2 first.";
    public string unlockPrompt = "Press [E] to unlock";
    public HingeDoor hingeDoor;
    public GameObject objectToDisableOnUnlock;
    public GameObject objectToRevealOnUnlock;
    public GameObject wallToDisableOnCollect;
    public Animator anim;
    public string openTriggerName = "Open";

    [Header("Victory UI Settings")]
    public bool triggerVictoryOnUnlock = false;
    public GameObject victoryPanel;

    public string GetPrompt()
    {
        if (itemType == ItemType.Key)
        {
            return string.IsNullOrEmpty(interactPrompt) ? $"Press [E] to pick up {itemLabel}" : interactPrompt;
        }

        bool hasKey = GameManager.Instance != null && GameManager.Instance.HasKey(requiredKeyNumber);
        if (!hasKey)
        {
            return lockedMessage;
        }

        return unlockPrompt;
    }

    public string Interact()
    {
        if (itemType == ItemType.Key)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectKey();
            }

            if (wallToDisableOnCollect != null)
            {
                wallToDisableOnCollect.SetActive(false);
            }

            gameObject.SetActive(false);
            return "";
        }

        if (itemType == ItemType.LockedDoorOrChest)
        {
            bool hasKey = GameManager.Instance != null && GameManager.Instance.HasKey(requiredKeyNumber);
            if (hasKey)
            {
                if (hingeDoor != null)
                {
                    hingeDoor.OpenDoor();
                }

                if (anim != null && !string.IsNullOrEmpty(openTriggerName))
                {
                    anim.SetTrigger(openTriggerName);
                }

                if (objectToDisableOnUnlock != null)
                {
                    objectToDisableOnUnlock.SetActive(false);
                }

                if (objectToRevealOnUnlock != null)
                {
                    objectToRevealOnUnlock.SetActive(true);
                }

                if (triggerVictoryOnUnlock)
                {
                    ShowVictoryUI();
                }

                return "";
            }
            else
            {
                Debug.Log(lockedMessage);
                return lockedMessage;
            }
        }

        return "";
    }

    private void ShowVictoryUI()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}