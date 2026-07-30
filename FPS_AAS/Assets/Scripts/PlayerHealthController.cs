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
        // currentHealth = maxHealth;

        if (UIController.instance != null)
        {
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text = currentHealth + "/" + maxHealth;

            // 1. Показываем начальное количество аптечек в UI
            UIController.instance.UpdateMedkitCount(medkitsCount);
        }
    }

    void Update()
    {
        if (invincibleCounter > 0)
        {
            invincibleCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TryUseMedkit();
        }
    }

    void TryUseMedkit()
    {
        // Нет аптечек
        if (medkitsCount <= 0)
        {
            if (UIController.instance != null)
                UIController.instance.ShowMessage("No medkits!");
            return;
        }

        // Полное здоровье
        if (currentHealth >= maxHealth)
        {
            if (UIController.instance != null)
                UIController.instance.ShowMessage("Health is already full!");
            return;
        }

        // okee
        PlayerController.instance.PlaySFX(PlayerController.instance.usemedkitSound, 4f);
        // Применяем лечилку
        healPlayer(healAmountPerMedkit);
        medkitsCount--;

        // 2. Обновляем счетчик в UI и выводим сообщение
        if (UIController.instance != null)
        {
            UIController.instance.UpdateMedkitCount(medkitsCount);
         
        }
    }

    /// <summary>
    /// Метод для подбора аптечки с земли/триггера
    /// </summary>
    public bool AddMedkit(int amount = 1)
    {
        // Если уже максимальное количество аптечек
        if (medkitsCount >= maxMedkits)
        {
            if (UIController.instance != null)
                UIController.instance.ShowMessage("Medkits are full!");
            return false; // Не смогли подобрать
        }

        medkitsCount += amount;
        if (medkitsCount > maxMedkits)
        {
            medkitsCount = maxMedkits;
        }

        // 3. Обновляем UI при подборе
        if (UIController.instance != null)
        {
            UIController.instance.UpdateMedkitCount(medkitsCount);
            
        }

        return true; // Успешно подобрали
    }

    public void healPlayer(int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (UIController.instance != null)
        {
            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text = currentHealth + "/" + maxHealth;
        }
    }

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer)
        {
            if (invincibleCounter <= 0)
            {
                currentHealth -= damage;
                PlayerController.instance.PlayRandomHitSound();

                // Показываем эффекты получения урона
                if (UIController.instance != null)
                    UIController.instance.ShowHitEffect();

                if (currentHealth <= 0)
                {
                    transform.parent.gameObject.SetActive(false);
                    currentHealth = 0;
                    currentHealth = 0;

                    PlayerController.instance.PlaySFX(PlayerController.instance.deathSound);

                    if (GameManager.instance != null)
                        GameManager.instance.PlayerDied();
                }
            }

            invincibleCounter = invicncibleLength;

            if (UIController.instance != null)
            {
                UIController.instance.healthSlider.value = currentHealth;
                UIController.instance.healthText.text = currentHealth + "/" + maxHealth;
            }
        }
    }
}