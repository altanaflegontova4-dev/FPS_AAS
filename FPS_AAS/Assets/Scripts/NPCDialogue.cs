using System.Collections;
using UnityEngine;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines; // все фразы NPC
    public string promptText = "Press E to talk";

    [Header("Animation")]
    public Animator anim;
    public string talkTrigger = "Talk";
    public string idleTrigger = "Idle";

    [Header("Settings")]
    public float autoAdvanceTime = 4f; // секунд до автоскипа
    public bool canSkip = true;

    private bool isTalking = false;
    private int currentLine = 0;
    private Coroutine dialogueCoroutine;

    public string GetPromptText()
    {
        if (isTalking) return "Press E to skip";
        return promptText;
    }

    public void Interact()
    {
        if (!isTalking)
        {
            // начать диалог
            dialogueCoroutine = StartCoroutine(PlayDialogue());
        }
        else if (canSkip)
        {
            // скипнуть текущую фразу
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = StartCoroutine(NextLine());
        }
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;
        currentLine = 0;

        // анимация разговора
        if (anim != null)
            anim.SetTrigger(talkTrigger);

        // показываем первую фразу
        ShowLine(currentLine);

        yield return new WaitForSeconds(autoAdvanceTime);

        // автоматически идём по фразам
        while (currentLine < dialogueLines.Length - 1)
        {
            dialogueCoroutine = StartCoroutine(NextLine());
            yield return dialogueCoroutine;
        }

        EndDialogue();
    }

    IEnumerator NextLine()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            yield break;
        }

        ShowLine(currentLine);
        yield return new WaitForSeconds(autoAdvanceTime);
    }

    void ShowLine(int index)
    {
        if (index >= dialogueLines.Length) return;
        UIController.instance.ShowDialogue(dialogueLines[index]);
    }

    void EndDialogue()
    {
        isTalking = false;
        currentLine = 0;

        // возврат в idle
        if (anim != null)
            anim.SetTrigger(idleTrigger);

        UIController.instance.HideDialogue();
    }
}