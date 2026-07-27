using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator anim;
    private Transform player;

    [Header("Damage Setting Combat")]
    public int chargeDamage = 10;
    public int slamDamage = 12;

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    private bool isRaged = false; // Фаза 2 (<50% HP)

    [Header("Tactical Spacing")]
    public float personalSpace = 5f;    // Если игрок подошел вплотную — босс отталкивает или бьет
    public float optimalRange = 12f;    // Оптимальная дистанция для ведения боя
    public float maxChaseRange = 18f;   // Если игрок дальше — босс делает таран или бежит сокращать дистанцию
    public float attackCooldown = 1.2f; // Пауза между атаками для создания напряжения!
    private float cooldownTimer = 0f;

    [Header("Line of Sight (Walls Check)")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    [Header("Boss Bullet Pool")]
    public BulletController bulletPrefab;
    public int bulletPoolSize = 40;
    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform bulletPoolParent;
    private bool bulletPoolReady;

    [Header("Weapon / Fire Points")]
    public Transform[] firePoints;
    public int bulletsPerBurst = 5;
    public float burstInterval = 0.12f;
    public float bulletSpawnDelay = 0.4f;
    public float aimOffsetAngle = 0f;

    private float fireCount, shotWaitCounter, ShootTimeCounter;
    public float fireRate = 1.35f;
    public float waitBetweenShots = 1f;
    public float timeToShoot = 2f;
    private bool hasStoppedForAttack = false;

    private bool isAttacking = false;
    private float stunTimer = 0f;

    private float normalSpeed;
    private float rageSpeed;

    void Start()
    {
        currentHealth = maxHealth;
        ShootTimeCounter = timeToShoot;

        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

        if (agent != null)
        {
            normalSpeed = agent.speed;
            rageSpeed = normalSpeed * 1.35f;
            agent.stoppingDistance = 6f; // ВАЖНО: Босс больше не липнет к лицу игрока!
        }

        PrepareBulletPool();
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Обработка оглушения / получения урона
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f && agent != null && gameObject.activeInHierarchy)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
            return;
        }

        // Переход в фазу ярости
        if (!isRaged && currentHealth <= maxHealth * 0.5f)
        {
            EnterRageMode();
        }

        if (isAttacking) return;

        // Таймер передышки (чтобы босс не спамил атаки как пулемет, а давал игроку напряженный момент)
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            StopAgentAndFacePlayer();
            anim.SetBool("IsMoving", false);
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Проверка видимости через стены
        if (!HasLineOfSight())
        {
            ChasePlayer(2f); // Если игрок за стеной, подбегаем ближе
            return;
        }

        // --- ТАКТИЧЕСКИЙ ИИ (СОЗДАНИЕ НАПРЯЖЕНИЯ) ---

        if (distToPlayer <= personalSpace)
        {
            // 1. ИГРОК СЛИШКОМ БЛИЗКО: Босс наказывает за наглость мощным ударом вблизи!
            StopAgentAndFacePlayer();
            StartCoroutine(LeapSlamAttack());
        }
        else if (distToPlayer <= optimalRange)
        {
            // 2. ОПТИМАЛЬНАЯ ЗОНА: Босс держит дистанцию (не лезет в лицо!) и ведет прицельный огонь
            StopAgentAndFacePlayer();
            HandleShootingRhythm(distToPlayer);
        }
        else if (distToPlayer <= maxChaseRange)
        {
            // 3. ИГРОК ОТХОДИТ: Босс просто идет за ним, сохраняя дистанцию остановки (6 метров)
            ChasePlayer(6f);
        }
        else
        {
            // 4. ИГРОК УБЕЖАЛ ДАЛЕКО: Идеальный момент для смертоносного тарана!
            if (isRaged || UnityEngine.Random.value < 0.75f)
            {
                StartCoroutine(ChargeAttack());
            }
            else
            {
                ChasePlayer(5f);
            }
        }
    }

    void ChasePlayer(float stopDist)
    {
        hasStoppedForAttack = false;
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.stoppingDistance = stopDist;
            agent.destination = player.position;
            anim.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);
        }
    }

    void StopAgentAndFacePlayer()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(direction);
            Quaternion correctedRotation = baseRotation * Quaternion.Euler(0, aimOffsetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, Time.deltaTime * 10f);
        }
    }

    void HandleShootingRhythm(float dist)
    {
        if (!hasStoppedForAttack)
        {
            shotWaitCounter = 0f;
            fireCount = 0f;
            ShootTimeCounter = timeToShoot;
            hasStoppedForAttack = true;
        }

        if (shotWaitCounter > 0)
        {
            shotWaitCounter -= Time.deltaTime;
            anim.SetBool("IsMoving", false);
        }
        else
        {
            ShootTimeCounter -= Time.deltaTime;
            if (ShootTimeCounter > 0)
            {
                fireCount -= Time.deltaTime;
                if (fireCount <= 0)
                {
                    StartCoroutine(ShootBurst(bulletSpawnDelay));
                    fireCount = isRaged ? fireRate * 0.5f : fireRate;
                }
            }
            else
            {
                shotWaitCounter = isRaged ? waitBetweenShots * 0.6f : waitBetweenShots;
            }
            anim.SetBool("IsMoving", false);
        }
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;
        Vector3 startPos = transform.position + Vector3.up * eyeHeight;
        Vector3 endPos = player.position + Vector3.up * 0.5f;

        RaycastHit hit;
        if (Physics.Linecast(startPos, endPos, out hit, obstacleMask))
        {
            Debug.DrawLine(startPos, hit.point, Color.red);
            if (!hit.transform.CompareTag("Player")) return false;
        }
        else
        {
            Debug.DrawLine(startPos, endPos, Color.green);
        }
        return true;
    }

    void EnterRageMode()
    {
        isRaged = true;
        if (agent != null) agent.speed = rageSpeed;
        attackCooldown = 0.6f; // В ярости передышки короче!
        Debug.Log("БОСС В ЯРОСТИ! Скорость и агрессия повышены.");
    }

    // --- АТАКА 1: Стрельба очередьми ---
    IEnumerator ShootBurst(float initialDelay)
    {
        isAttacking = true;
        anim.SetTrigger("fireShot");
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (player == null || !gameObject.activeInHierarchy) yield break;

            foreach (Transform fp in firePoints)
            {
                if (fp == null) continue;
                Vector3 targetCenter = player.position + new Vector3(0f, 0.4f, 0f);
                Vector3 dir = targetCenter - fp.position;
                if (dir.sqrMagnitude < 0.01f) continue;

                GetBullet(fp.position, Quaternion.LookRotation(dir));
            }

            if (i < bulletsPerBurst - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        yield return new WaitForSeconds(0.4f);
        FinishAttack(isRaged ? 0.5f : attackCooldown);
    }

    // --- Attack 2: Leap Slam
    IEnumerator LeapSlamAttack()
    {
        isAttacking = true;
        anim.SetTrigger("LeapSlam");
        if (agent != null) agent.enabled = false;

        yield return new WaitForSeconds(1.5f);

        float slamRadius = 4.5f;
        if (player != null && Vector3.Distance(transform.position, player.position) <= slamRadius)
        {
            IDamagable player = PlayerController.instance.GetComponentInChildren<IDamagable>();

            if (player != null)
            {
                player.TakeDamage(slamDamage, true);
                Debug.Log("Player is damaged");
            }
        }

        yield return new WaitForSeconds(0.6f);
        if (agent != null && !isDead)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
        FinishAttack(attackCooldown * 1.2f); // После тяжелого прыжка босс дольше приходит в себя
    }

    // --- Attack 3: Charge
    IEnumerator ChargeAttack()
    {
        isAttacking = true;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsCharging", true);
        anim.SetTrigger("Charge");

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        Vector3 frozenPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            transform.position = frozenPosition;
            yield return null;
        }

        if (agent != null && !isDead)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = rageSpeed * 5.0f;
            agent.acceleration = 50f;
            agent.stoppingDistance = 2.0f;
            agent.updateRotation = true;

            if (player != null)
                agent.destination = player.position;
        }

        float chargeDuration = 2.0f;
        elapsed = 0f;
        bool hitPlayer = false;

        while (elapsed < chargeDuration && !isDead)
        {
            elapsed += Time.deltaTime;

          
                if (player != null && agent != null && agent.enabled)
                {
                    agent.destination = player.position;

                    float dist = Vector3.Distance(transform.position, player.position);
                    Debug.Log("Дистанция до игрока: " + dist); // добавь это

                    if (dist <= 4.0f)
                    {
                    // урон...
                    hitPlayer = true;
                    Debug.Log("Дистанция подходящая — пытаемся нанести урон");

                    IDamagable playerDamagable =
                        PlayerController.instance.GetComponentInChildren<IDamagable>();

                    if (playerDamagable != null)
                    {
                        Debug.Log("IDamagable найден — наносим урон " + chargeDamage);
                        playerDamagable.TakeDamage(chargeDamage, true);
                    }
                }
                }


                yield return null;
        }

        anim.SetBool("IsCharging", false);
        anim.SetBool("IsMoving", false);

        if (agent != null && !isDead)
        {
            agent.speed = isRaged ? rageSpeed : normalSpeed;
            agent.acceleration = 12f;
            agent.stoppingDistance = 6f;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (hitPlayer && !isDead)
        {
            anim.SetTrigger("LeapSlam");
            yield return new WaitForSeconds(1.0f);
        }

        FinishAttack(attackCooldown * 1.5f);
    }

    // Вызывается в конце каждой атаки, чтобы задать паузу (ритм боя)
    void FinishAttack(float cooldown)
    {
        isAttacking = false;
        cooldownTimer = cooldown;
        hasStoppedForAttack = false;
    }

    public void StunByHit(float duration)
    {
        StopAllCoroutines();
        isAttacking = false;
        stunTimer = duration;
        hasStoppedForAttack = false;

        if (agent != null)
        {
            agent.enabled = false;
        }

        anim.SetTrigger("Hit");
    }

    public void CancelAttacks()
    {
        StopAllCoroutines();
        isAttacking = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;

        StunByHit(0.25f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        if (agent != null) agent.enabled = false;
        anim.SetTrigger("Die");
        Debug.Log("БОСС ПОВЕРЖЕН!");
    }

    // --- Пул пуль ---
    private void PrepareBulletPool()
    {
        if (bulletPoolReady) return;
        GameObject parentObj = new GameObject(gameObject.name + "_BossBulletPool");
        bulletPoolParent = parentObj.transform;

        for (int i = 0; i < bulletPoolSize; i++)
        {
            BulletController bullet = Instantiate(bulletPrefab, bulletPoolParent);
            bullet.gameObject.SetActive(false);
            bullet.SetReturnAction(ReturnBullet);
            bulletPool.Enqueue(bullet);
        }
        bulletPoolReady = true;
    }

    private BulletController GetBullet(Vector3 position, Quaternion rotation)
    {
        PrepareBulletPool();
        BulletController bullet = bulletPool.Count > 0 ? bulletPool.Dequeue() : Instantiate(bulletPrefab, bulletPoolParent);
        bullet.SetReturnAction(ReturnBullet);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
        bullet.Fire();
        return bullet;
    }

    private void ReturnBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}