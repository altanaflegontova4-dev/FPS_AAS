using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    public static UIController instance;


    [Header("Objective")]
    public GameObject objectivePanel;
    public Text objectiveText;

    [Header("Health")]
    public Slider healthSlider;
    public Text healthText;

    [Header("Boss Health")]
    public Slider bossHealthSlider;
    public Text bossHealthText;
    public GameObject bossHealthPanel; 

    [Header("Ammo")]
    public Text ammoText;
    public UnityEngine.UI.Image weaponIconImage;

    [Header("Medkit UI")]
    public Text medkitCountText;

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
    public float idleScale = 1f;    
    public float walkScale = 1.35f; 
    public float scaleSpeed = 10f;  
    private float targetScale;


    [Header("Hit Effect")]
    public UnityEngine.Rendering.Volume hitVolume; 
    private UnityEngine.Rendering.Universal.Vignette hitVignette;
    private Coroutine hitEffectCoroutine;

    public void Awake()
    {
        instance = this;
    }

    void Start()
    {
        HideInteractPrompt();
        HideMessage();

        if (hitVolume != null)
            hitVolume.profile.TryGet(out hitVignette);
    }

    void Update()
    {
        if (crosshair != null)
        {
            float current = Mathf.Lerp(crosshair.transform.localScale.x, targetScale, Time.deltaTime * scaleSpeed);
            crosshair.transform.localScale = new Vector3(current, current, 1f);
        }

        if (notePanel != null && notePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideNote();
            }
        }
    }


    /// <summary>
    /// Обновить текущую цель на экране. Если передать пустую строку "", панель скроется.
    /// </summary>
    public void UpdateObjective(string newObjective)
    {
        if (string.IsNullOrEmpty(newObjective))
        {
            HideObjective();
            return;
        }

        if (objectivePanel != null)
            objectivePanel.SetActive(true);

        if (objectiveText != null)
            objectiveText.text = newObjective;
    }



    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIconImage != null)
        {
            if (icon != null)
            {
                weaponIconImage.gameObject.SetActive(true);
                weaponIconImage.sprite = icon;
            }
            else
            {
                // Если иконка не задана (например, для ножа), скрываем её
                weaponIconImage.gameObject.SetActive(false);
            }
        }
    }

    // Метод обновления количества аптечек
    public void UpdateMedkitCount(int count)
    {
        if (medkitCountText != null)
        {
            medkitCountText.text = count.ToString();
        }
    }



    public void HideObjective()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);
    }

    public void SetCrosshairWalking(bool isWalking, bool isMelee)
    {
        if (crosshair != null)
        {
            // Прицел должен быть активен ТОЛЬКО если это не нож и не открыта записка
            bool isNoteOpen = notePanel != null && notePanel.activeSelf;
            bool shouldBeActive = !isMelee && !isNoteOpen;

            if (crosshair.activeSelf != shouldBeActive)
            {
                crosshair.SetActive(shouldBeActive);
            }
        }

        targetScale = isWalking ? walkScale : idleScale;
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
            healthText.text = ""+ currentHealth;
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

        // При закрытии записки включаем прицел ТОЛЬКО если в руках НЕ нож
        if (crosshair != null)
        {
            bool isMelee = PlayerController.instance != null &&
                           PlayerController.instance.activeGun != null &&
                           PlayerController.instance.activeGun.isMelee;

            crosshair.SetActive(!isMelee);
        }

        // скрываем курсор обратно
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // разблокируем игрока
        PlayerController.instance.enabled = true;
    }

    public void ShowHitEffect()
    {
        if (hitEffectCoroutine != null)
            StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(HitEffectCoroutine());
    }

    private IEnumerator HitEffectCoroutine()
    {
        if (hitVignette == null) yield break;

        // включаем красную виньетку
        hitVignette.color.Override(Color.red);
        hitVignette.intensity.Override(0.6f);

        yield return new WaitForSeconds(0.1f);

        // плавно убираем
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            hitVignette.intensity.Override(Mathf.Lerp(0.6f, 0f, t));
            yield return null;
        }

        hitVignette.intensity.Override(0f);
    }
}