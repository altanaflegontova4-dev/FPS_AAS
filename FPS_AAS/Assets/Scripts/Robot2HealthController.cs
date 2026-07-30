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

    [Header("Death Effects")]
    public ParticleSystem smokeEffect;
    public ParticleSystem electricEffect;
    public Transform[] effectSpawnPoints;

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer || isDead) return;

        currentHealth -= damage;

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

        PlayDeathEffects();

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


    void PlayDeathEffects()
    {
        if (effectSpawnPoints != null && effectSpawnPoints.Length > 0)
        {
            foreach (Transform point in effectSpawnPoints)
            {
                if (smokeEffect != null)
                {
                    ParticleSystem smoke = Instantiate(smokeEffect, point.position, point.rotation);
                    Destroy(smoke.gameObject, 2.0f);
                }

                if (electricEffect != null)
                {
                    ParticleSystem electric = Instantiate(electricEffect, point.position, point.rotation);
                    Destroy(electric.gameObject, 2.0f);
                }
            }
        }
        else
        {
            if (smokeEffect != null)
            {
                ParticleSystem smoke = Instantiate(smokeEffect, transform.position + Vector3.up, transform.rotation);
                Destroy(smoke.gameObject, 2.0f);
            }

            if (electricEffect != null)
            {
                ParticleSystem electric = Instantiate(electricEffect, transform.position + Vector3.up, transform.rotation);
                Destroy(electric.gameObject, 2.0f);
            }
        }
    }
}