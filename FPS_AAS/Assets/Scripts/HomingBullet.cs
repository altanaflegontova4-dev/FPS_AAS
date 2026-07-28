using System;
using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float turnSpeed = 3f; // скорость поворота к игроку
    public float lifeTime = 4f;
    public int damage = 15;
    public bool attackPlayer = true;

    [Header("Effects")]
    public GameObject impactEffect;

    private Transform target;
    private float lifeCounter;
    private bool hasHit;
    private float ignoreCollisionTimer = 0.1f;
    private Rigidbody rb;

    private Action<HomingBullet> returnToPool;

    public void SetReturnAction(Action<HomingBullet> returnAction)
    {
        returnToPool = returnAction;
    }

    public void Fire()
    {
        hasHit = false;
        lifeCounter = lifeTime;
        ignoreCollisionTimer = 0.1f;

        // находим игрока как цель
        if (PlayerController.instance != null)
            target = PlayerController.instance.transform;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * moveSpeed;
        }
    }

    void Update()
    {
        lifeCounter -= Time.deltaTime;
        if (lifeCounter <= 0)
        {
            ReturnToPool();
            return;
        }

        if (ignoreCollisionTimer > 0)
            ignoreCollisionTimer -= Time.deltaTime;

        // самонаведение — поворачиваемся к игроку каждый кадр
        if (target != null && rb != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // плавно поворачиваем пулю к игроку
            Vector3 newDirection = Vector3.RotateTowards(
                transform.forward,
                directionToTarget,
                turnSpeed * Time.deltaTime,
                0f
            );

            transform.rotation = Quaternion.LookRotation(newDirection);
            rb.linearVelocity = transform.forward * moveSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (ignoreCollisionTimer > 0) return;

        hasHit = true;

        IDamagable damageable = other.GetComponentInParent<IDamagable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, attackPlayer);
        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (returnToPool != null)
            returnToPool(this);
        else
            gameObject.SetActive(false);
    }
}