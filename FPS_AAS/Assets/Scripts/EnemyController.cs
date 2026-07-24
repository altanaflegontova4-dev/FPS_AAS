using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private bool chasing;

    public float distanceToChase = 10f, distanceToLose = 15f, distanceToStop = 2f, distanceToShoot = 7f;
    private Vector3 targetPoint, originalPoint;

    public NavMeshAgent agent;

    public float keepChasingTime = 5f;
    private float chaseCounter;

    [Header("Enemy Bullet Pool")]
    public BulletController bulletPrefab;
    public int bulletPoolSize = 30; // Увеличили пул, так как пуль теперь летит больше

    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform bulletPoolParent;
    private bool bulletPoolReady;

    public Transform firePoint;

    public float fireRate = 1.35f, waitBetweenShots = 1f, timeToShoot = 1f;
    private float fireCount, shotWaitCounter, ShootTimeCounter;

    [Header("Burst Settings")]
    public int bulletsPerBurst = 3;      // Количество пуль в одной очереди
    public float burstInterval = 0.15f;  // Скорострельность внутри очереди

    [Header("Animation Sync")]
    public float bulletSpawnDelay = 0.3f;

    [Header("Aim Correction")]
    public float aimOffsetAngle = 0f;

    public Animator anim;

    private bool hasStoppedForAttack = false;
    private float stunTimer = 0f;

    void Start()
    {
        originalPoint = transform.position;

        ShootTimeCounter = timeToShoot;
        shotWaitCounter = 0f;
        fireCount = 0f;
        hasStoppedForAttack = false;

        PrepareBulletPool();
    }

    public void StunByHit(float duration)
    {
        StopAllCoroutines(); // Отменяем текущие очереди и выстрелы при получении урона
        stunTimer = duration;
        chasing = false;
    }

    public void CancelAttacks()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        // Если робот оглушен — пропускаем логику
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                if (agent != null && !agent.enabled && gameObject.activeInHierarchy)
                {
                    agent.enabled = true;
                }
            }
            return;
        }

        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        if (!chasing)
        {
            agent.updateRotation = true;
            hasStoppedForAttack = false;

            if (chaseCounter > 0)
            {
                agent.destination = transform.position;
                chaseCounter -= Time.deltaTime;
            }
            else
            {
                agent.destination = originalPoint;
            }

            if (Vector3.Distance(transform.position, targetPoint) <= distanceToChase)
            {
                chasing = true;
                ShootTimeCounter = timeToShoot;
                shotWaitCounter = 0f;
                fireCount = 0f;
                hasStoppedForAttack = false;
            }

            if (agent.remainingDistance < .25f)
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
                chaseCounter = keepChasingTime;
                hasStoppedForAttack = false;
            }

            float triggerDistance = hasStoppedForAttack ? (distanceToShoot + 2f) : distanceToShoot;

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

                // Плавный поворот с поправкой Mixamo
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

                                // Запускаем корутину очереди пуль
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
                agent.updateRotation = true;
                agent.destination = targetPoint;
                shotWaitCounter = 0f;
                fireCount = 0f;
                hasStoppedForAttack = false;
                anim.SetBool("IsMoving", true);
            }
        }
    }

    private System.Collections.IEnumerator ShootBurst(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (PlayerController.instance == null || !gameObject.activeInHierarchy) yield break;

            Vector3 playerCenter = PlayerController.instance.transform.position + new Vector3(0f, 0.4f, 0f);
            Vector3 directionToPlayer = (playerCenter - firePoint.position).normalized;
            Quaternion accurateRotation = Quaternion.LookRotation(directionToPlayer);

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