using UnityEngine;

public class EnemyHealthController : MonoBehaviour, IDamagable
{
    public int currentHealth = 5;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void damage (int damageAmount)
    {

    }

    public void TakeDamage(int damage, bool attackPlayer)
    {
        Debug.Log("Attacking enemy");
        if (!attackPlayer)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
