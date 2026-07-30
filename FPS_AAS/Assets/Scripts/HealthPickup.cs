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
                PlayerController.instance.PlaySFX(PlayerController.instance.healthfullSound,2f);
                UIController.instance.ShowMessage("Medkits capacity is full!");
                return;
            }

            // add medkit
            PlayerHealthController.instance.medkitsCount++;
            PlayerController.instance.PlaySFX(PlayerController.instance.pickupSound,7f);

            UIController.instance.ShowMessage("Picked up a Medkit!");

            Destroy(gameObject);
        }
    }
}