using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointController : MonoBehaviour
{
    public string cpName;
  
    void Start()
    {
        if(PlayerPrefs.HasKey(SceneManager.GetActiveScene().name + "_cp"))
        {
            if (PlayerPrefs.GetString(SceneManager.GetActiveScene().name + "_cp") == cpName)
            {
                PlayerController.instance.GetComponent<CharacterController>().enabled = false;
                PlayerController.instance.transform.position = transform.position;
                PlayerController.instance.transform.rotation = transform.rotation;
                PlayerController.instance.GetComponent<CharacterController>().enabled = true;

                Debug.Log("Player Starting at: " + cpName);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            PlayerPrefs.SetString (SceneManager.GetActiveScene().name + "_cp", cpName);
            Debug.Log("Player hit = " + cpName);
        }
    }
}
