using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Health")]
    public Slider healthSlider;
    public Text healthText;

    [Header("Boss Health")]
    public Slider bossHealthSlider;
    public Text bossHealthText;
    public GameObject bossHealthPanel; 

    [Header("Ammo")]
    public Text ammoText;

    [Header("Interact")]
    public Text interactPromptText; 
    public GameObject interactPromptPanel; 

    [Header("Message")]
    public Text messageText; 
    public GameObject messagePanel;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public Text dialogueText;

    [Header("DialoguePA")]
    public GameObject dialoguePanelPA;
    public Text dialogueTextPA;
    public Text speakerNameText;

    [Header("Note")]
    public GameObject notePanel;
    public Text noteTitleText;
    public Text noteBodyText;
    public UnityEngine.UI.Image noteImageUI;

    [Header("Crosshair")]
    public GameObject crosshair;

    public void Awake()
    {
        instance = this;
    }

    void Start()
    {
        HideInteractPrompt();
        HideMessage();
    }

    void Update()
    {
        if (notePanel != null && notePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideNote();
            }
        }
    }

    public void ShowInteractPrompt(string text)
    {
        if (interactPromptPanel != null)
            interactPromptPanel.SetActive(true);

        if (interactPromptText != null)
            interactPromptText.text = text;
    }

    public void HideInteractPrompt()
    {
        if (interactPromptPanel != null)
            interactPromptPanel.SetActive(false);

        if (interactPromptText != null)
            interactPromptText.text = "";
    }

    public void ShowMessage(string message)
    {
        StartCoroutine(ShowMessageCoroutine(message));
    }

    private IEnumerator ShowMessageCoroutine(string message)
    {
        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (messageText != null)
            messageText.text = message;
        else
            ammoText.text = message; 

        yield return new WaitForSeconds(1.0f);

        HideMessage();
        PlayerController.instance.activeGun.UpdateAmmoUI();
    }

    public void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (messageText != null)
            messageText.text = "";
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = "HP: " + currentHealth;
    }

    public void ShowDialogue(string text)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (dialogueText != null)
            dialogueText.text = "";
    }

    public void ShowDialoguePA(string speaker, string text)
    {
        if (dialoguePanelPA != null)
            dialoguePanelPA.SetActive(true);
        if (speakerNameText != null)
            speakerNameText.text = speaker;
        if (dialogueTextPA != null)
            dialogueTextPA.text = text;
    }

    public void HideDialoguePA()
    {
        if (dialoguePanelPA != null)
            dialoguePanelPA.SetActive(false);
    }

    public void ShowNote(string title, string body, Sprite image = null)
    {
        if (notePanel != null)
            notePanel.SetActive(true);

        if (noteTitleText != null)
            noteTitleText.text = title;

        if (noteBodyText != null)
            noteBodyText.text = body;

        if (noteImageUI != null && image != null)
            noteImageUI.sprite = image;

        // скрываем crosshair
        if (crosshair != null)
            crosshair.SetActive(false);

        // показываем курсор чтобы можно было закрыть
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideNote()
    {
        if (notePanel != null)
            notePanel.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(true);

        // скрываем курсор обратно
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // разблокируем игрока
        PlayerController.instance.enabled = true;
    }
}