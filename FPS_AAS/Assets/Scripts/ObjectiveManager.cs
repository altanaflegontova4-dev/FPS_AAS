using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

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
}