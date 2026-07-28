using UnityEngine;

public class CursorController : MonoBehaviour
{
    void Start()
    {
        // Hides the cursor and locks it to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Optional: Press Escape to reveal the cursor again during gameplay
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}