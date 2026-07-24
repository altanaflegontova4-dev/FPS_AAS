using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthController : MonoBehaviour, IDamagable
{
    public int currentHealth = 5;

    public Animator anim;

    public MleeRobot1Controller enemyController;

    private bool isDead;

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
}