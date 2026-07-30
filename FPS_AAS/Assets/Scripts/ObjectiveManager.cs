using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    public Door exitDoor;
    public int notesCollected = 0;
    public int requiredNotes = 4;

    public bool survivorRescued = false;

    private int nodesDestroyed = 0;
    private int totalNodes = 3;

    void Awake()
    {
        instance = this;
    }

    public void NodeDestroyed()
    {
        nodesDestroyed++;

        UIController.instance.ShowMessage(
            "Node destroyed: " + nodesDestroyed + "/" + totalNodes);

        if (nodesDestroyed >= totalNodes)
        {
            // все nodes уничтожены
            UIController.instance.ShowMessage(
                "All nodes destroyed! The Overseer is weakened.");

            // открыть дверь к боссу
            // BossDoor.instance.Open();
        }
    }

    public bool CanExit()
    {
        return notesCollected >= requiredNotes && survivorRescued;
    }

    public void CollectNote()
    {
        notesCollected++;

        UIController.instance.ShowMessage("Scraped Notes: " + notesCollected + "/" + requiredNotes);

        if (notesCollected >= requiredNotes && survivorRescued)
        {
            exitDoor.Unlock();
        }
    }

    public void RescueSurvivor()
    {
        survivorRescued = true;

        UIController.instance.ShowMessage("Survivor rescued!");

        if (notesCollected >= requiredNotes)
        {
            exitDoor.Unlock();
        }
    }


}