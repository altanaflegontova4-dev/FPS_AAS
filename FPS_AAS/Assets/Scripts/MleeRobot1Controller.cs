using UnityEngine;
using UnityEngine.AI;

public class MleeRobot1Controller : MonoBehaviour
{
    private bool chasing;

    public float distanceToChase = 5f;
    public float distanceToLose = 7f;
    public float attackRange = 1.2f;

    private Vector3 targetPoint;
    private Vector3 originalPoint;

    public NavMeshAgent agent;

    public float keepChasingTime = 5f;
    private float chaseCounter;

    public Animator anim;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;
    private bool isAttacking;

    public float attackDuration = 1.2f;
    private float attackDurationTimer;

    public int attackDamage = 1;

    void Start()
    {
        originalPoint = transform.position;
    }

    void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (isAttacking)
        {
            // Безопасно меняем параметры агента только если он активен
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            transform.LookAt(new Vector3(targetPoint.x, transform.position.y, targetPoint.z));
            anim.SetBool("IsMoving", false);

            return;
        }

        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint);

        if (chasing)
        {
            if (distanceToPlayer > distanceToLose)
            {
                chasing = false;
                chaseCounter = keepChasingTime;
                if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
            }
            else
            {
                if (distanceToPlayer <= attackRange)
                {
                    if (attackTimer <= 0)
                    {
                        Attack();
                    }
                    else
                    {
                        if (agent.enabled && agent.isOnNavMesh)
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                        }
                        transform.LookAt(new Vector3(targetPoint.x, transform.position.y, targetPoint.z));
                    }
                }
                else
                {
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(targetPoint);
                    }
                }
            }
        }
        else
        {
            if (distanceToPlayer <= attackRange || distanceToPlayer <= distanceToChase)
            {
                chasing = true;
            }
            else
            {
                if (chaseCounter > 0)
                {
                    chaseCounter -= Time.deltaTime;
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.ResetPath();
                        agent.isStopped = true;
                    }
                }
                else
                {
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(originalPoint);

                        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                        {
                            agent.ResetPath();
                        }
                    }
                }
            }
        }

        bool isMoving = agent != null && agent.enabled && agent.isOnNavMesh && !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
        anim.SetBool("IsMoving", isMoving);
    }

    void Attack()
    {
        transform.LookAt(new Vector3(targetPoint.x, targetPoint.y, targetPoint.z));

        if (attackTimer <= 0 && !isAttacking)
        {
            isAttacking = true;

            if (agent != null)
            {
                agent.enabled = false;
            }

            int attackChoice = Random.Range(0, 2);
            if (attackChoice == 0)
            {
                anim.SetTrigger("Punch");
            }
            else
            {
                anim.SetTrigger("Kick");
            }

            attackTimer = attackCooldown;

            StartCoroutine(DealDamageDelayed(0.4f));
            StartCoroutine(FinishAttackRoutine(attackDuration));
        }
    }

    private System.Collections.IEnumerator DealDamageDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PlayerController.instance == null) yield break;

        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatPlayerPos = new Vector3(PlayerController.instance.transform.position.x, 0, PlayerController.instance.transform.position.z);
        float distance = Vector3.Distance(flatEnemyPos, flatPlayerPos);

        if (distance <= attackRange + 0.5f)
        {
            IDamagable player = PlayerController.instance.GetComponentInChildren<IDamagable>();

            if (player != null)
            {
                player.TakeDamage(attackDamage, true);
                Debug.Log("✅ УРОН УСПЕШНО НАНЕСЕН ИГРОКУ!");
            }
            else
            {
                Debug.LogError("❌ ОШИБКА: У объекта игрока нет компонента с интерфейсом IDamagable!");
            }
        }
    }

    private System.Collections.IEnumerator FinishAttackRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        isAttacking = false;

        if (agent != null && gameObject.activeSelf)
        {
            agent.enabled = true;
        }
    }

    public void StunByHit(float stunDuration)
    {
        StartCoroutine(HitStunRoutine(stunDuration));
    }

    private System.Collections.IEnumerator HitStunRoutine(float duration)
    {
        isAttacking = true;

        if (agent != null)
        {
            agent.enabled = false;
        }

        anim.SetBool("IsMoving", false);

        yield return new WaitForSeconds(duration);

        if (agent != null && gameObject.activeSelf)
        {
            agent.enabled = true;
        }
        isAttacking = false;
    }
}