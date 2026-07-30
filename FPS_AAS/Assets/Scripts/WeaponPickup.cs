using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Weapon Pickup Settings")]
    public Gun gunPrefab;          // Префаб оружия, которое мы подбираем
    public string weaponName = "Shotgun"; // Название для UI текста

    public string GetPromptText()
    {
        return "Press E to pick up " + weaponName;
    }

    public void Interact()
    {
        if (PlayerController.instance != null && gunPrefab != null)
        {
            PlayerController.instance.PlaySFX(PlayerController.instance.pickupSound, 2f);


            // Передаем префаб оружия игроку
            PlayerController.instance.AddWeapon(gunPrefab);

            // Удаляем лежащее на земле оружие
            Destroy(gameObject);
        }
    }
}