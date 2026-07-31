using UnityEngine;

public class ScrapNoteManager : MonoBehaviour
{
    public static ScrapNoteManager instance;

    public int requiredNotes = 4;

    private int collectedNotes = 0;

    public AudioSource audioSource;
    public AudioClip nextlevelSound;

    public int CollectedNotes => collectedNotes;

    void Awake()
    {
        instance = this;
    }

    public void CollectScrapNote()
    {
        collectedNotes++;

        UIController.instance.ShowMessage(
            "Scraped Notes: " + collectedNotes + "/" + requiredNotes);

        if (collectedNotes >= requiredNotes)
        {
            UIController.instance.ShowMessage(
                "You found all Scraped Notes. Rescue the survivor to unlock the door.");

            if (audioSource != null && nextlevelSound != null)
                audioSource.PlayOneShot(nextlevelSound);
        }
    }
}