using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthController : MonoBehaviour, IDamagable
{
    public int currentHealth = 5;

    public Animator anim;

    public MleeRobot1Controller enemyController;
   

    private bool isDead;

    [Header("Death Effects")]
    public ParticleSystem smokeEffect;
    public ParticleSystem electricEffect;
    public Transform[] effectSpawnPoints;

    // Длительность анимации получения урона в секундах (подкрути под свою анимацию Hit)
    public float hitStunDuration = 0.6f;

    void Start()
    {

    }

    public void damage(int damageAmount)
    {

    }

    public void TakeDamage(int damage, bool attackPlayer)
    {
        Debug.Log("Attacking enemy");

        if (attackPlayer || isDead)
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log("Enemy Health: " + currentHealth);

        // 1. ПРОВЕРЯЕМ СМЕРТЬ В ПЕРВУЮ ОЧЕРЕДЬ!
        if (currentHealth <= 0)
        {
            Die();
            return; // ВАЖНО: выходим из метода мгновенно, чтобы не включился Hit!
        }

        // 2. Если робот ЕЩЕ ЖИВ — тогда уже включаем анимацию боли и стан
        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetTrigger("Hit");
        }


        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

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

        // Отключаем скрипт управления
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        // Отключаем NavMeshAgent
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // ВАЖНО: Отключаем коллайдер, чтобы невидимая капсула не держала тело в воздухе!
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // ВАЖНО: Отключаем гравитацию и физику, чтобы тело не провалилось сквозь пол и не подпрыгивало
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // НЕ уничтожаем сразу
        Destroy(gameObject, 5f);
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
                    Destroy(smoke.gameObject, 2.5f);
                }

                if (electricEffect != null)
                {
                    ParticleSystem electric = Instantiate(electricEffect, point.position, point.rotation);
                    Destroy(electric.gameObject, 2.5f);
                }
            }
        }
        else
        {
            if (smokeEffect != null)
            {
                ParticleSystem smoke = Instantiate(smokeEffect, transform.position + Vector3.up, transform.rotation);
                Destroy(smoke.gameObject, 2.5f);
            }

            if (electricEffect != null)
            {
                ParticleSystem electric = Instantiate(electricEffect, transform.position + Vector3.up, transform.rotation);
                Destroy(electric.gameObject, 2.5f);
            }
        }
    }
}