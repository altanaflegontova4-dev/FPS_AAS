using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EnemyController : MonoBehaviour
{
    private bool chasing;

    public float distanceToChase = 10f, distanceToLose = 15f, distanceToStop = 2f, distanceToShoot = 7f;
    private Vector3 targetPoint, originalPoint;

    public NavMeshAgent agent;

    public float keepChasingTime = 5f;
    private float chaseCounter;

    [Header("Audio")]
    AudioSource AS;

    public AudioClip spottedSound;
    public AudioClip robotshootSound;
    public AudioClip[] footstepSounds;
    public AudioClip robotgothitSound;

    private float footstepTimer;
    public float footstepDelay = 0.45f;

    [Header("Line of Sight")]
    public LayerMask obstacleMask; // Слой стен/препятствий (НЕ включать игрока и врага!)
    public float eyeHeight = 1f;   // Высота точки пуска луча от врага

    [Header("Enemy Bullet Pool")]
    public BulletController bulletPrefab;
    public int bulletPoolSize = 30;

    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform bulletPoolParent;
    private bool bulletPoolReady;

    public Transform firePoint;

    public float fireRate = 1.35f, waitBetweenShots = 1f, timeToShoot = 1f;
    private float fireCount, shotWaitCounter, ShootTimeCounter;

    [Header("Burst Settings")]
    public int bulletsPerBurst = 3;
    public float burstInterval = 0.15f;

    [Header("Animation Sync")]
    public float bulletSpawnDelay = 0.3f;

    [Header("Aim Correction")]
    public float aimOffsetAngle = 0f;

    public Animator anim;

    private bool hasStoppedForAttack = false;
    private float stunTimer = 0f;

    // Флаг для отслеживания боя в CombatManager
    private bool hasSpottedPlayer = false;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        AS = GetComponent<AudioSource>();

        originalPoint = transform.position;

        ShootTimeCounter = timeToShoot;
        shotWaitCounter = 0f;
        fireCount = 0f;
        hasStoppedForAttack = false;

        PrepareBulletPool();
    }

    public void PlayHitSound()
    {
        if (AS != null && robotgothitSound != null)
        {
            AS.PlayOneShot(robotgothitSound, 1.2f);
        }
    }
    public void StunByHit(float duration)
    {


        StopAllCoroutines();
        stunTimer = duration;

        if (chasing)
        {
            chasing = false;
            EndCombat(); // При оглушении выходим из боя
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.destination = transform.position; // останавливаем движение
        }
    }

    public void CancelAttacks()
    {
        StopAllCoroutines();
    }

    // Проверка линии видимости: два луча (центр и голова игрока)
    bool HasLineOfSight()
    {
        if (PlayerController.instance == null) return false;

        Vector3 startPos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerCenter = PlayerController.instance.transform.position + new Vector3(0f, 0.8f, 0f); // примерно центр тела
        Vector3 playerEye = PlayerController.instance.transform.position + new Vector3(0f, 1.6f, 0f);    // уровень глаз

        if (!CheckRay(startPos, playerCenter)) return false;
        if (!CheckRay(startPos, playerEye)) return false;

        return true;
    }

    bool CheckRay(Vector3 start, Vector3 end)
    {
        RaycastHit hit;
        if (Physics.Linecast(start, end, out hit, obstacleMask))
        {
            Debug.DrawLine(start, hit.point, Color.red);
            return false;
        }

        Debug.DrawLine(start, end, Color.green);
        return true;
    }

    void Update()
    {
        if (PlayerController.instance == null) return;

        // Если оглушён — пропускаем логику
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f && agent != null && !agent.enabled && gameObject.activeInHierarchy)
            {
                agent.enabled = true;
            }
            return;
        }

        // Получаем позицию игрока (плоская для навигации)
        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        // Проверка по вертикали
        float verticalDiff = Mathf.Abs(transform.position.y - PlayerController.instance.transform.position.y);
        const float maxVerticalDiffForSight = 4.0f;

        bool canSee = HasLineOfSight();
        bool isVerticallyClose = verticalDiff <= maxVerticalDiffForSight;

        // Если нет видимости ИЛИ большая вертикальная разница — останавливаем врага
        if (!canSee || !isVerticallyClose)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.destination = transform.position;
            }
            anim.SetBool("IsMoving", false);


            if (chasing)
            {
                chasing = false;
                hasStoppedForAttack = false;
                EndCombat(); // Потеряли из виду — снимаем бой
            }

            return;
        }

        // Логика начала/продолжения погони
        if (!chasing)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.updateRotation = true;
            }
            hasStoppedForAttack = false;

            if (chaseCounter > 0)
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh) agent.destination = transform.position;
                chaseCounter -= Time.deltaTime;
            }
            else
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh) agent.destination = originalPoint;
            }

            float distToPlayer = Vector3.Distance(transform.position, targetPoint);
            if (distToPlayer <= distanceToChase)
            {
                chasing = true;
                StartCombat(); // Заметил игрока — начинаем бой!
                ShootTimeCounter = timeToShoot;
                shotWaitCounter = 0f;
                fireCount = 0f;
                hasStoppedForAttack = false;
            }

            if (agent != null && agent.enabled && agent.isOnNavMesh && agent.remainingDistance < .25f)
            {
                anim.SetBool("IsMoving", false);
            }
            else
            {
                anim.SetBool("IsMoving", true);
            }
        }
        else
        {
            float distToPlayer = Vector3.Distance(transform.position, targetPoint);

            if (distToPlayer > distanceToLose)
            {
                chasing = false;
                EndCombat(); // Игрок убежал слишком далеко
                chaseCounter = keepChasingTime;
                hasStoppedForAttack = false;

                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.destination = transform.position;
                }
                anim.SetBool("IsMoving", false);
                return;
            }

            float triggerDistance = hasStoppedForAttack ? (distanceToShoot + 2f) : distanceToShoot;

            // Атака: только если видим игрока
            if (distToPlayer <= triggerDistance && canSee)
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.destination = transform.position;
                    agent.updateRotation = false;
                }

                if (!hasStoppedForAttack)
                {
                    shotWaitCounter = 0f;
                    fireCount = 0f;
                    ShootTimeCounter = timeToShoot;
                    hasStoppedForAttack = true;
                }

                // Поворот к игроку
                Vector3 direction = (targetPoint - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion baseRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    Quaternion correctedRotation = baseRotation * Quaternion.Euler(0, aimOffsetAngle, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, Time.deltaTime * 12f);
                }

                if (shotWaitCounter > 0)
                {
                    shotWaitCounter -= Time.deltaTime;
                    if (shotWaitCounter <= 0)
                    {
                        ShootTimeCounter = timeToShoot;
                    }
                    anim.SetBool("IsMoving", false);
                }
                else if (PlayerController.instance.gameObject.activeInHierarchy)
                {
                    ShootTimeCounter -= Time.deltaTime;

                    if (ShootTimeCounter > 0)
                    {
                        fireCount -= Time.deltaTime;

                        if (fireCount <= 0)
                        {
                            fireCount = fireRate;

                            Vector3 targetDir = PlayerController.instance.transform.position - transform.position;
                            float angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);

                            if (Math.Abs(angle) <= 45)
                            {
                                anim.SetTrigger("fireShot");
                                StartCoroutine(ShootBurst(bulletSpawnDelay));
                            }
                        }
                    }
                    else
                    {
                        shotWaitCounter = waitBetweenShots;
                    }

                    anim.SetBool("IsMoving", false);
                }
            }
            else
            {
                // Погоня: идём к игроку, если не атакуем
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.updateRotation = true;
                    agent.destination = targetPoint;
                }
                shotWaitCounter = 0f;
                fireCount = 0f;
                hasStoppedForAttack = false;
                anim.SetBool("IsMoving", true);
            }
        }

        bool isMoving = agent != null &&
                agent.enabled &&
                agent.isOnNavMesh &&
                !agent.isStopped &&
                agent.velocity.sqrMagnitude > 0.01f;

        if (isMoving && footstepSounds.Length > 0)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                int index = UnityEngine.Random.Range(0, footstepSounds.Length);
                AS.PlayOneShot(footstepSounds[index], 0.8f);

                footstepTimer = footstepDelay;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private IEnumerator ShootBurst(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (PlayerController.instance == null || !gameObject.activeInHierarchy) yield break;

            Vector3 playerCenter = PlayerController.instance.transform.position + new Vector3(0f, 0.4f, 0f);
            Vector3 directionToPlayer = (playerCenter - firePoint.position).normalized;
            Quaternion accurateRotation = Quaternion.LookRotation(directionToPlayer);

            AS.PlayOneShot(robotshootSound, 1.5f);

            GetBullet(firePoint.position, accurateRotation);

            if (i < bulletsPerBurst - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
    }

    private void PrepareBulletPool()
    {
        if (bulletPoolReady) return;

        GameObject parentObj = new GameObject(gameObject.name + "_EnemyBulletPool");
        parentObj.transform.SetParent(transform);
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

    // ========================================================================
    // ЛОГИКА СВЯЗИ С COMBAT MANAGER
    // ========================================================================

    private void StartCombat()
    {
        if (!hasSpottedPlayer)
        {
            hasSpottedPlayer = true;

            if (spottedSound != null)
            {
                AS.PlayOneShot(spottedSound, 2f);
            }

            if (CombatManager.instance != null)
            {
                CombatManager.instance.RegisterEnemy();
            }
        }
    }

    private void EndCombat()
    {
        if (hasSpottedPlayer)
        {
            hasSpottedPlayer = false;
            if (CombatManager.instance != null)
            {
                CombatManager.instance.UnregisterEnemy();
            }
        }
    }

    private void OnDisable()
    {
        EndCombat();
    }

    private void OnDestroy()
    {
        EndCombat();
    }
}