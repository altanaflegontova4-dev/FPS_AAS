using UnityEngine;

public class SurvivorManager : MonoBehaviour
{
    public static SurvivorManager instance;

    public bool survivorRescued = false;

    private void Awake()
    {
        instance = this;
    }

    public void RescueSurvivor()
    {
        ObjectiveManager.instance.RescueSurvivor();
    }
}