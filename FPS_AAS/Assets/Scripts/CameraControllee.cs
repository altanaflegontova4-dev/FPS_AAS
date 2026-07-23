using UnityEngine;

public class CameraControllee : MonoBehaviour
{
    public Transform target;
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
      transform.position = target.position;//to attach CameraPoint to target variable
      transform.rotation = target.rotation;
    }


}
