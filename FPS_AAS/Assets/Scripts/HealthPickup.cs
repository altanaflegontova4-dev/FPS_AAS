using UnityEngine;

public class HealthPickup : MonoBehaviour, IInteractable
{

    public string GetPromptText()
    {
        return "Press E to pick up Medkit";
    }

    public void Interact()
    {
        if (PlayerHealthController.instance != null)
        {
            // check if have space
            if (PlayerHealthController.instance.medkitsCount >= PlayerHealthController.instance.maxMedkits)
            {
                UIController.instance.ShowMessage("Medkits capacity is full!");
                return;
            }

            // add medkit
            PlayerHealthController.instance.medkitsCount++;

            UIController.instance.ShowMessage("Picked up a Medkit!");

            Destroy(gameObject);
        }
    }
}