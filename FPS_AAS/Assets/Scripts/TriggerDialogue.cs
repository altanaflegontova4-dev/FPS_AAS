using System.Collections;
using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Settings")]
    public float autoAdvanceTime = 3f;
    public bool canSkip = true;
    public bool triggerOnce = true;

    private bool hasTriggered = false;
    private bool isTalking = false;
    private int currentLine = 0;
    private Coroutine dialogueCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (isTalking) return;

        hasTriggered = true;
        dialogueCoroutine = StartCoroutine(PlayDialogue());
    }

    void Update()
    {
        if (isTalking && canSkip && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = StartCoroutine(NextLine());
        }
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;
        currentLine = 0;

        ShowLine(currentLine);
        yield return new WaitForSeconds(autoAdvanceTime);

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

        string line = dialogueLines[index];
        string[] parts = line.Split(':');

        if (parts.Length >= 2)
        {
            string speaker = parts[0].Trim();
            string text = parts[1].Trim();
            UIController.instance.ShowDialoguePA(speaker, text); // используем PA версию
        }
        else
        {
            UIController.instance.ShowDialoguePA("", line);
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        currentLine = 0;
        UIController.instance.HideDialoguePA(); // используем PA версию
    }
}