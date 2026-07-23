using UnityEngine;

public class OneWayDoor : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    public Animator anim;

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
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        // Всегда запускаем только одну анимацию открытия
        anim.SetTrigger("Open");
        isOpen = true;
    }

    private void CloseDoor()
    {
        anim.SetTrigger("Close");
        isOpen = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            CloseDoor();
        }
    }
}