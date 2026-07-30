using UnityEngine;
using UnityEngine.AI;

public class ShootingEnemyHealthController : MonoBehaviour, IDamagable
{
    public int currentHealth = 5;
    public Animator anim;

    // Ссылка на контроллер СТРЕЛЯЮЩЕГО робота
    public EnemyController enemyController;

    private bool isDead;
    public float hitStunDuration = 0.6f;

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer || isDead) return;

        currentHealth -= damage;

        if (enemyController != null)
        {
            enemyController.PlayHitSound();
        }


        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetTrigger("Hit");
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        if (enemyController != null)
        {
            enemyController.StunByHit(hitStunDuration);
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        if (enemyController != null)
        {
            enemyController.CancelAttacks();
            enemyController.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        Destroy(gameObject, 2f);
    }
}