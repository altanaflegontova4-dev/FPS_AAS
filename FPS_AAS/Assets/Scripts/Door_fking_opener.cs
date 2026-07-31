using System.Collections;
using UnityEngine;

// Attach this to any GameObject in the scene (e.g. an empty "DoorManager",
// or on one of the doors themselves).
// Assign door1 and door2 in the Inspector.

public class DoorRotate : MonoBehaviour
{
    [Header("Doors")]
    public Transform door1;   // will rotate +90 on Y
    public Transform door2;   // will rotate -90 on Y

    [Header("Settings")]
    public float rotateAngle = 90f;
    public float openDuration = 4f;      // how long the door stays open
    public float rotateTime = 0.5f;      // seconds it takes to swing open/closed
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    public Camera playerCamera;

    private Quaternion door1ClosedRot, door1OpenRot;
    private Quaternion door2ClosedRot, door2OpenRot;
    private bool isAnimating = false;
    private Coroutine currentRoutine;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        door1ClosedRot = door1.rotation;
        door1OpenRot = door1.rotation * Quaternion.Euler(0f, rotateAngle, 0f);

        door2ClosedRot = door2.rotation;
        door2OpenRot = door2.rotation * Quaternion.Euler(0f, -rotateAngle, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && !isAnimating && IsLookingAtDoor())
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(OpenThenClose());
        }
    }

    bool IsLookingAtDoor()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            return hit.transform == door1 || hit.transform == door2
                || hit.transform.IsChildOf(door1) || hit.transform.IsChildOf(door2);
        }
        return false;
    }

    IEnumerator OpenThenClose()
    {
        isAnimating = true;

        // Open both doors
        yield return RotateBoth(door1OpenRot, door2OpenRot);

        // Wait while open
        yield return new WaitForSeconds(openDuration);

        // Close both doors
        yield return RotateBoth(door1ClosedRot, door2ClosedRot);

        isAnimating = false;
    }

    IEnumerator RotateBoth(Quaternion target1, Quaternion target2)
    {
        Quaternion start1 = door1.rotation;
        Quaternion start2 = door2.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / rotateTime;
            door1.rotation = Quaternion.Slerp(start1, target1, t);
            door2.rotation = Quaternion.Slerp(start2, target2, t);
            yield return null;
        }

        door1.rotation = target1;
        door2.rotation = target2;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
