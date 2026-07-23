using UnityEngine;

public class TargetMovement : MonoBehaviour
{

    public bool shouldMove, shouldRotate;
    public float moveSpeed, rotateSpeed;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        if (shouldMove)
        {
            transform.position += new Vector3(moveSpeed, 0f, 0f) * Time.deltaTime;
        }

        if (shouldRotate)
        {
            //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, rotateSpeed * Time.deltaTime, 0f));
            Vector3 turnEnemy= new Vector3(0f, rotateSpeed, 0f);
            transform.Rotate (turnEnemy * Time.deltaTime);
        }

    }
}
