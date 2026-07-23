using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public string GetPromptText()
    {
        return "Press E to open door";
    }

    public void Interact()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
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