using UnityEngine;

public class NoteInteractable : MonoBehaviour, IInteractable
{
    [Header("Note Settings")]
    public string promptText = "Press E to read";

    [Header("Note Content")]
    public Sprite noteImage; // картинка записки если есть
    [TextArea(5, 15)]
    public string noteText; // текст записки
    public string noteTitle; // заголовок записки


    [Header("Objective")]
    public bool isScrapedNote = false;
    private bool collected = false;
    private bool isReading = false;

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact()
    {
        if (!isReading)
        {
            isReading = true;
            PlayerController.instance.PlaySFX(PlayerController.instance.noteSound, 2f);

            UIController.instance.ShowNote(noteTitle, noteText, noteImage);
            // блокируем игрока пока читает
            PlayerController.instance.enabled = false;
        }

        if (isScrapedNote && !collected)
        {
            collected = true;

            ObjectiveManager.instance.CollectNote();
            ScrapNoteManager.instance.CollectScrapNote();
        }
    }
}