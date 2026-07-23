using UnityEngine;

public class InteractSystem : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public Transform camTrans;

    void Update()
    {
        // показываем подсказку если смотрим на объект
        CheckForInteractable();

        // нажали E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void CheckForInteractable()
    {
        RaycastHit hit;
        // Стреляем лучом
        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, interactRange, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Если нашли предмет - показываем текст
                UIController.instance.ShowInteractPrompt(interactable.GetPromptText());
            }
            else
            {
                // Если объект есть (в слое), но скрипта нет - прячем
                UIController.instance.HideInteractPrompt();
            }
        }
        else
        {
            // Если вообще ничего не попало в луч - прячем
            UIController.instance.HideInteractPrompt();
        }
    }

    void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTrans.position, camTrans.forward,
            out hit, interactRange, interactLayer))
        {
            IInteractable interactable =
                hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}