using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    public Door exitDoor;
    public int notesCollected = 0;
    public int requiredNotes = 4;

    public bool survivorRescued = false;

    [Header("Progress")]
    public int nodesDestroyed = 0;
    public int totalNodes = 3;

    [Header("Objective Texts")]
    public string initialObjectiveText = "Destroy network nodes";
    public string completedObjectiveText = "Proceed to the Boss Chamber";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //Show current objective
        UpdateObjectiveUI();
    }

    public void NodeDestroyed()
    {
        nodesDestroyed++;

        // Check progression
        if (nodesDestroyed >= totalNodes)//if all nodes done
        {
            // Сообщение по центру/сбоку
            if (UIController.instance != null)
            {
                UIController.instance.ShowMessage("All nodes destroyed! The Overseer is weakened.");

                // Change objective
                UIController.instance.UpdateObjective(completedObjectiveText);
            }

     
        }
        else //if not all nodes doen
        {
            UpdateObjectiveUI();
        }
    }

    private void UpdateObjectiveUI()
    {
        if (UIController.instance != null)
        {
            string fullObjective = initialObjectiveText + " (" + nodesDestroyed + "/" + totalNodes + ")";
            UIController.instance.UpdateObjective(fullObjective);
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