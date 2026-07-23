using UnityEngine;

public class AmmoPickup : MonoBehaviour, IInteractable
{
    public string ammoType;
    public int ammoAmount;

    public string GetPromptText()
    {
        return "Press E to pick up ammo for [" + ammoType + "]"; 
    }

    public void Interact()
    {
        foreach (Gun gun in PlayerController.instance.allGuns)
        {
            if (gun.ammoType == ammoType)
            {
                if (gun.reserveAmmo >= gun.maxReserveAmmo)
                {
                    UIController.instance.ShowMessage("Ammo is full!");
                    return;
                }

                gun.AddAmmo(ammoAmount);
                UIController.instance.ShowMessage("You picked up " + ammoAmount + " " + ammoType + " ammo!");
                Destroy(gameObject);
                return;
            }
        }
    }
}