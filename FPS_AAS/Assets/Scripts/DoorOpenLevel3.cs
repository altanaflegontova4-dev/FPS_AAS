using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Door : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    private Animator anim;

    [Header("Lock")]
    [SerializeField] private bool locked = false;

    [Header("Exit Door Lvl 2")]
    [SerializeField] private bool isExitDoorLvl2 = false;
    [SerializeField] private float levelLoadDelay = 1.5f;

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(levelLoadDelay);
        GameManager.instance.Level3_Altana();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public string GetPromptText()
    {
        if (isExitDoorLvl2)
        {
            if (ObjectiveManager.instance.notesCollected < ObjectiveManager.instance.requiredNotes)
                return $"Need {ScrapNoteManager.instance.requiredNotes} Scraped Notes and rescue a Survivor.";

            if (!ObjectiveManager.instance.survivorRescued)
                return "Rescue the survivor first.";
        }

        return "Press E to open door";
    }

    public void Unlock()
    {
        locked = false;
    }


    private void OpenDoor()
    {
        Vector3 playerToDoor = (transform.position - Camera.main.transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, playerToDoor);

        ResetAllTriggers();

        if (dotProduct > 0)
            anim.SetTrigger("OpenForward");
        else
            anim.SetTrigger("OpenBackward");

        isOpen = true;
    }

    public void Interact()
    {
        if (locked)
        {
            UIController.instance.ShowMessage("The door is locked.");
            return;
        }

        if (isExitDoorLvl2)
        {
            if (!ObjectiveManager.instance.CanExit())
            {
                UIController.instance.ShowMessage("Complete all objectives first.");
                return;
            }
        }

        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            PlayerController.instance.PlaySFX(PlayerController.instance.doorSound);
            OpenDoor();

            if (isExitDoorLvl2)
                StartCoroutine(LoadNextLevel());
        }

    }

    private void CloseDoor()
    {
        ResetAllTriggers();
        anim.SetTrigger("Close");
        isOpen = false;
    }

    private void ResetAllTriggers()
    {
        anim.ResetTrigger("OpenForward");
        anim.ResetTrigger("OpenBackward");
        anim.ResetTrigger("Close");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            CloseDoor();
        }
    }
}

    
