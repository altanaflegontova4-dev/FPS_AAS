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
            UIController.instance.ShowNote(noteTitle, noteText, noteImage);
            // блокируем игрока пока читает
            PlayerController.instance.enabled = false;
        }
    }
}