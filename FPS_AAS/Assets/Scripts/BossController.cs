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

    [Header("Boss State")]
    public bool bossActivated = false;

    [Header("Damage Setting Combat")]
    public int chargeDamage = 10;
    public int slamDamage = 12;
    public float pushForce = 10f;

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    private bool isRaged = false;

    [Header("Tactical Spacing")]
    public float personalSpace = 5f;
    public float optimalRange = 12f;
    public float maxChaseRange = 18f;
    public float attackCooldown = 1.2f;
    private float cooldownTimer = 0f;

    [Header("Line of Sight")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    [Header("Normal Bullet Pool")]
    public BulletController bulletPrefab;
    public int bulletPoolSize = 40;
    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform bulletPoolParent;
    private bool bulletPoolReady;

    [Header("Homing Bullet Pool")]
    public HomingBullet homingBulletPrefab;
    public int homingPoolSize = 15;
    private Queue<HomingBullet> homingPool = new Queue<HomingBullet>();
    private Transform homingPoolParent;
    private bool homingPoolReady;

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
            agent.stoppingDistance = 6f;
        }

        PrepareBulletPool();
        PrepareHomingPool();
    }

    void Update()
    {
        if (!bossActivated) return;
        if (isDead || player == null) return;

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

        if (!isRaged && currentHealth <= maxHealth * 0.5f)
        {
            EnterRageMode();
        }

        if (isAttacking) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            StopAgentAndFacePlayer();
            anim.SetBool("IsMoving", false);
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (!HasLineOfSight())
        {
            ChasePlayer(2f);
            return;
        }

        if (distToPlayer <= personalSpace)
        {
            StopAgentAndFacePlayer();
            StartCoroutine(LeapSlamAttack());
        }
        else if (distToPlayer <= optimalRange)
        {
            StopAgentAndFacePlayer();
            HandleShootingRhythm(distToPlayer);
        }
        else if (distToPlayer <= maxChaseRange)
        {
            ChasePlayer(6f);
        }
        else
        {
            if (isRaged || UnityEngine.Random.value < 0.75f)
                StartCoroutine(ChargeAttack());
            else
                ChasePlayer(5f);
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

    public void EnterRageMode()
    {
        isRaged = true;
        if (agent != null) agent.speed = rageSpeed;
        attackCooldown = 0.6f;

       
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        Debug.Log("SkinnedMeshRenderers found: " + renderers.Length);

        foreach (SkinnedMeshRenderer r in renderers)
        {
            // создаём копию материала чтобы не менять оригинал
            Material[] mats = r.materials;
            foreach (Material mat in mats)
            {
                Debug.Log("Material: " + mat.name + " shader: " + mat.shader.name);
                //mat.SetColor("_BaseColor", new Color(0.91f, 0.62f, 0.62f, 1f));
                // также меняем цвет самой текстуры overlay
                mat.SetColor("_EmissionColor", new Color(0.75f, 0f, 0f));
                mat.EnableKeyword("_EMISSION");
            }
            r.materials = mats;
        }

        Debug.Log("БОСС В ЯРОСТИ!");
    }

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

                if (isRaged)
                {
                    GetHomingBullet(fp.position, fp.rotation);
                }
                else
                {
                    Vector3 targetCenter = player.position + new Vector3(0f, 0.4f, 0f);
                    Vector3 dir = targetCenter - fp.position;
                    if (dir.sqrMagnitude < 0.01f) continue;
                    GetBullet(fp.position, Quaternion.LookRotation(dir));
                }
            }

            if (i < bulletsPerBurst - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        yield return new WaitForSeconds(0.4f);
        FinishAttack(isRaged ? 0.5f : attackCooldown);
    }

    IEnumerator LeapSlamAttack()
    {
        isAttacking = true;
        anim.SetTrigger("LeapSlam");
        if (agent != null) agent.enabled = false;

        yield return new WaitForSeconds(1.5f);

        float slamRadius = 4.5f;
        if (player != null && Vector3.Distance(transform.position, player.position) <= slamRadius)
        {
            IDamagable playerDamagable = PlayerController.instance.GetComponentInChildren<IDamagable>();
            if (playerDamagable != null)
                playerDamagable.TakeDamage(slamDamage, true);
        }

        yield return new WaitForSeconds(0.6f);
        if (agent != null && !isDead)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
        FinishAttack(attackCooldown * 1.2f);
    }

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

                if (dist <= 3f)
                {
                    hitPlayer = true;

                    IDamagable playerDamagable =
                        PlayerController.instance.GetComponentInChildren<IDamagable>();
                    if (playerDamagable != null)
                        playerDamagable.TakeDamage(chargeDamage, true);

                    PlayerController.instance.ApplyPush(
                        (player.position - transform.position).normalized + Vector3.up * 0.2f,
                        pushForce
                    );

                    break;
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
            agent.enabled = false;

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
            Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        if (agent != null) agent.enabled = false;
        anim.SetTrigger("Die");
    }

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
        BulletController bullet = bulletPool.Count > 0
            ? bulletPool.Dequeue()
            : Instantiate(bulletPrefab, bulletPoolParent);
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

    private void PrepareHomingPool()
    {
        if (homingPoolReady) return;
        GameObject parentObj = new GameObject(gameObject.name + "_HomingBulletPool");
        homingPoolParent = parentObj.transform;

        for (int i = 0; i < homingPoolSize; i++)
        {
            HomingBullet bullet = Instantiate(homingBulletPrefab, homingPoolParent);
            bullet.gameObject.SetActive(false);
            bullet.SetReturnAction(ReturnHomingBullet);
            homingPool.Enqueue(bullet);
        }
        homingPoolReady = true;
    }

    private HomingBullet GetHomingBullet(Vector3 position, Quaternion rotation)
    {
        PrepareHomingPool();
        HomingBullet bullet = homingPool.Count > 0
            ? homingPool.Dequeue()
            : Instantiate(homingBulletPrefab, homingPoolParent);
        bullet.SetReturnAction(ReturnHomingBullet);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
        bullet.Fire();
        return bullet;
    }

    private void ReturnHomingBullet(HomingBullet bullet)
    {
        bullet.gameObject.SetActive(false);
        homingPool.Enqueue(bullet);
    }
}