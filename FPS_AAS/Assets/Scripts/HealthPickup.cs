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
            // AddMedkit сам проверит лимит, обновит UI и вернет true, если подбор удался
            bool success = PlayerHealthController.instance.AddMedkit(1);

            if (success)
            {
                PlayerController.instance.PlaySFX(PlayerController.instance.pickupSound, 6f);

                Destroy(gameObject);
            }
        }
    }
}