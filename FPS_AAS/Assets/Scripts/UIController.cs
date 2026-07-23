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

    [Header("Ammo")]
    public Text ammoText;

    [Header("Interact")]
    public Text interactPromptText; 
    public GameObject interactPromptPanel; 

    [Header("Message")]
    public Text messageText; 
    public GameObject messagePanel;

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
}