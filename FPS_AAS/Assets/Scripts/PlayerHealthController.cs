using UnityEngine;

public class PlayerHealthController : MonoBehaviour, IDamagable
{

    public static PlayerHealthController instance;

    public float invicncibleLength = 1f;
    private float invincibleCounter;

    public int maxHealth, currentHealth;

    [Header("Medkits")]
    public int medkitsCount = 0;    
    public int maxMedkits = 3;        
    public int healAmountPerMedkit = 10; 

    public void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //currentHealth = maxHealth;

        UIController.instance.healthSlider.maxValue = maxHealth;
        UIController.instance.healthSlider.value = currentHealth;
        UIController.instance.healthText.text= "HEALTH: " + currentHealth + "/"+maxHealth;


    }


    void Update()
    {
        if (invincibleCounter>0)
        {
            invincibleCounter-=Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TryUseMedkit();
        }


    }

    public void DamagePlayer (int damageAmount)
    {
        


    }

    void TryUseMedkit()
    {
       //no medkits
        if (medkitsCount <= 0)
        {
            PlayerController.instance.PlaySFX(PlayerController.instance.healthfullSound, 2f);
            UIController.instance.ShowMessage("No medkits!");
            return;
        }

        // if full health
        if (currentHealth >= maxHealth)
        {
            PlayerController.instance.PlaySFX(PlayerController.instance.healthfullSound, 2f);
            UIController.instance.ShowMessage("Health is already full!");
            return;
        }

        // okee
        PlayerController.instance.PlaySFX(PlayerController.instance.usemedkitSound, 4f);
        healPlayer(healAmountPerMedkit);
        medkitsCount--;

        UIController.instance.ShowMessage("Used medkit! " + medkitsCount + " left.");
        // Позже сюда можно будет добавить обновление иконки аптечек на экране
    }


    public void healPlayer (int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UIController.instance.healthSlider.value = currentHealth;
        UIController.instance.healthText.text = "HEALTH: " + currentHealth + "/" + maxHealth;
    }

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer)
        {
            if (invincibleCounter <= 0)
            {
                currentHealth -= damage;
                PlayerController.instance.PlayRandomHitSound();

                if (currentHealth <= 0)
                {
                    currentHealth = 0;

                    PlayerController.instance.PlaySFX(PlayerController.instance.deathSound);

                    transform.parent.gameObject.SetActive(false);
                    GameManager.instance.PlayerDied();
                }
            }

            invincibleCounter = invicncibleLength;

            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text = "HEALTH: " + currentHealth + "/" + maxHealth;
        }
    }
}

