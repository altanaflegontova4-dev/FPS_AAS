using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
    

{
    public static GameManager instance;

    public float WaitAfterDying = 2f;

    private void Awake()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Level1_Sayat()
    {
        SceneManager.LoadScene("Level2_Aileen");
    }

    public void Level2_Aileen()
    {
        SceneManager.LoadScene("Level2_Aileen");
    }

    public void Level3_Altana()
    {
        SceneManager.LoadScene("Level3_Altana");
    }

    public void PlayerDied()
    {
        StartCoroutine(PlayerDiedCo());
    }

    public IEnumerator PlayerDiedCo()
    {
        yield return new WaitForSeconds(WaitAfterDying);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void exit()
    {
        Application.Quit();
    }
}
