using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

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
}