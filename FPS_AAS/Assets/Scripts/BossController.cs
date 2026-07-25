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

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    private bool isRaged = false; // Фаза 2 (Ярость при <50% HP)

    [Header("Distances")]
    public float distanceToShoot = 12f;
    public float distanceToLeap = 8f;
    public float distanceToCharge = 15f;

    [Header("Boss Bullet Pool")]
    public BulletController bulletPrefab;
    public int bulletPoolSize = 40;
    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform bulletPoolParent;
    private bool bulletPoolReady;

    [Header("Weapon / Fire Points & Rhythm")]
    public Transform[] firePoints; // Точки стрельбы на плечах/руках босса
    public int bulletsPerBurst = 5;
    public float burstInterval = 0.12f;
    public float bulletSpawnDelay = 0.4f;
    public float aimOffsetAngle = 0f;

    // Параметры темпа стрельбы и пауз (как у Robot 2)
    public float fireRate = 1.35f;
    public float waitBetweenShots = 1f;
    public float timeToShoot = 2f;
    private float fireCount, shotWaitCounter, ShootTimeCounter;
    private bool hasStoppedForAttack = false;

    private bool isAttacking = false;
    private float stunTimer = 0f;

    // Скорости движения
    private float normalSpeed;
    private float rageSpeed;

    void Start()
    {
        currentHealth = maxHealth;

        ShootTimeCounter = timeToShoot;
        shotWaitCounter = 0f;
        fireCount = 0f;
        hasStoppedForAttack = false;

        if (PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        if (agent != null)
        {
            normalSpeed = agent.speed;
            rageSpeed = normalSpeed * 1.4f;
        }

        PrepareBulletPool();
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Если босс оглушен от попадания
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f && agent != null && gameObject.activeInHierarchy)
            {
                agent.enabled = true;
            }
            return;
        }

        // Проверяем фазу Ярости (< 50% здоровья)
        if (!isRaged && currentHealth <= maxHealth * 0.5f)
        {
            EnterRageMode();
        }

        // Если босс сейчас выполняет уникальную атаку (прыжок или таран), не прерываем её
        if (isAttacking) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        float triggerDistance = hasStoppedForAttack ? (distanceToShoot + 2f) : distanceToShoot;

        // Логика удержания дистанции и ритма атак
        if (distToPlayer <= triggerDistance)
        {
            agent.destination = transform.position;
            agent.updateRotation = false;

            if (!hasStoppedForAttack)
            {
                shotWaitCounter = 0f;
                fireCount = 0f;
                ShootTimeCounter = timeToShoot;
                hasStoppedForAttack = true;
            }

            // Плавный поворот лицом к игроку с поправкой Mixamo
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion baseRotation = Quaternion.LookRotation(direction);
                Quaternion correctedRotation = baseRotation * Quaternion.Euler(0, aimOffsetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, Time.deltaTime * 10f);
            }

            // Логика таймеров стрельбы и пауз (как у Robot 2)
            if (shotWaitCounter > 0)
            {
                shotWaitCounter -= Time.deltaTime;
                if (shotWaitCounter <= 0)
                {
                    ShootTimeCounter = timeToShoot;
                }
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
                        fireCount = isRaged ? fireRate * 0.6f : fireRate;
                        ExecuteBossAttack(distToPlayer);
                    }
                }
                else
                {
                    shotWaitCounter = isRaged ? waitBetweenShots * 0.5f : waitBetweenShots;
                }

                anim.SetBool("IsMoving", false);
            }
        }
        else
        {
            agent.updateRotation = true;
            agent.destination = player.position;
            shotWaitCounter = 0f;
            fireCount = 0f;
            hasStoppedForAttack = false;
            anim.SetBool("IsMoving", true);
        }
    }

    void EnterRageMode()
    {
        isRaged = true;
        if (agent != null) agent.speed = rageSpeed;
        Debug.Log("БОСС ПЕРЕШЕЛ В ФАЗУ ЯРОСТИ!");
    }

    void ExecuteBossAttack(float distance)
    {
        // Случайный выбор от 0 до 2
        int attackChoice = UnityEngine.Random.Range(0, 3);

        // В фазе ярости у босса выше шанс сделать рывок или прыжок
        if (isRaged)
        {
            attackChoice = UnityEngine.Random.Range(0, 2); // 0 или 1 будут чаще провоцировать спец-атаки
        }

        // Смягчаем условия дистанции, чтобы атаки реально происходили
        if (distance <= distanceToLeap && attackChoice == 0)
        {
            StartCoroutine(LeapSlamAttack());
        }
        else if (distance >= 8f && attackChoice == 1)
        {
            StartCoroutine(ChargeAttack());
        }
        else
        {
            StartCoroutine(ShootBurst(bulletSpawnDelay));
        }
    }

    // --- АТАКА 1: Плечевой залп ---
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
                if (fp != null)
                {
                    Vector3 playerCenter = player.position + new Vector3(0f, 0.4f, 0f);
                    Vector3 directionToPlayer = (playerCenter - fp.position).normalized;
                    Quaternion accurateRotation = Quaternion.LookRotation(directionToPlayer);

                    GetBullet(fp.position, accurateRotation);
                }
            }

            if (i < bulletsPerBurst - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    // --- АТАКА 2: Прыжок-удар (Leap Slam) ---
    IEnumerator LeapSlamAttack()
    {
        isAttacking = true;
        anim.SetTrigger("LeapSlam");

        if (agent != null) agent.enabled = false;

        yield return new WaitForSeconds(0.8f);

        float slamRadius = 4f;
        if (player != null && Vector3.Distance(transform.position, player.position) <= slamRadius)
        {
            Debug.Log("Игрок задет ударной волной босса!");
        }

        yield return new WaitForSeconds(0.5f);

        if (agent != null && !isDead) agent.enabled = true;
        isAttacking = false;
    }

    // --- АТАКА 3: Таран (Charge) ---
    // --- АТАКА 3: Таран (Charge) ---
    IEnumerator ChargeAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Charge");

        yield return new WaitForSeconds(0.6f); // Время на анимацию приседа

        if (agent != null)
        {
            agent.enabled = true;
            // Устанавливаем четкую скорость для тарана (без бесконечного умножения)
            float baseSpd = isRaged ? rageSpeed : normalSpeed;
            agent.speed = baseSpd * 2.2f;

            // Включаем анимацию бега, чтобы ноги перебирались, а не было скольжения!
            anim.SetBool("IsMoving", true);
        }

        float chargeTime = 1.5f;
        float elapsed = 0f;

        while (elapsed < chargeTime && !isDead)
        {
            elapsed += Time.deltaTime;

            // Чтобы он подруливал за игроком во время рывка
            if (player != null && agent != null && agent.enabled)
            {
                agent.destination = player.position;
            }

            // Проверка столкновения с игроком
            if (player != null && Vector3.Distance(transform.position, player.position) < 2.2f)
            {
                Debug.Log("Босс протаранил игрока!");
                // Здесь можно нанести урон игроку
                break;
            }
            yield return null;
        }

        // Возвращаем нормальную скорость и выключаем бег в аниматоре
        if (agent != null)
        {
            agent.speed = isRaged ? rageSpeed : normalSpeed;
        }

        anim.SetBool("IsMoving", false);
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // --- Управление при уроне ---
    public void StunByHit(float duration)
    {
        StopAllCoroutines();
        isAttacking = false;
        stunTimer = duration;
        hasStoppedForAttack = false;
        if (agent != null) agent.enabled = false;
    }

    public void CancelAttacks()
    {
        StopAllCoroutines();
        isAttacking = false;
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

        BulletController bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab, bulletPoolParent);
            bullet.SetReturnAction(ReturnBullet);
        }

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