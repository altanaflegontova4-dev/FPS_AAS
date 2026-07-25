using UnityEngine;
using UnityEngine.AI;

public class BossHealthController : MonoBehaviour, IDamagable
{
    [Header("Health & Settings")]
    public int currentHealth = 100;
    public Animator anim;

    public BossController bossController;

    private bool isDead;
    public float hitStunDuration = 0.5f;
    public float destroyDelay = 6f;

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer || isDead) return;

        currentHealth -= damage;
        Debug.Log("Boss Health: " + currentHealth);

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

        if (bossController != null)
        {
            bossController.StunByHit(hitStunDuration);
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        if (bossController != null)
        {
            bossController.CancelAttacks();
            bossController.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Аккуратно опускаем босса на землю при смерти
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 4f))
        {
            transform.position = hit.point;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        Destroy(gameObject, destroyDelay);
    }
}