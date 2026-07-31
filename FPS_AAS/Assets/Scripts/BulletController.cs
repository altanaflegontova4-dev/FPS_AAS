using System;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float moveSpeed;
    public float lifeTime;
    public Rigidbody rb;
    public ParticleSystem impactEffect;

    public bool attackPlayer;
    public int damage;

    private float lifeCounter;
    private bool hasHit;

    // --- НОВОЕ: Таймер игнорирования столкновений при рождении ---
    private float ignoreCollisionTimer;
    private const float IGNORE_DURATION = 0.05f; // Игнорируем столкновения первые 0.05 сек
    // -------------------------------------------------------------

    private Action<BulletController> returnToPool;

    public void SetReturnAction(Action<BulletController> returnAction)
    {
        returnToPool = returnAction;
    }

    public void Fire()
    {
        hasHit = false;
        lifeCounter = lifeTime;

        // Сбрасываем таймер игнорирования при каждом выстреле
        ignoreCollisionTimer = IGNORE_DURATION;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = transform.forward * moveSpeed;
    }

    private void Update()
    {
        lifeCounter -= Time.deltaTime;

        if (lifeCounter <= 0)
        {
            ReturnToPool();
        }

        // Уменьшаем таймер игнорирования каждый кадр
        if (ignoreCollisionTimer > 0)
        {
            ignoreCollisionTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Если мы уже попали - выходим
        if (hasHit) return;

        // 2. ГЛАВНОЕ: Если таймер игнорирования еще не истек - выходим!
        // Пуля только родилась внутри босса, мы не даем ей умереть сразу.
        if (ignoreCollisionTimer > 0) return;

        // Теперь, когда пуля вылетела из босса, проверяем остальное
        hasHit = true;

        IDamagable damageable = other.GetComponentInParent<IDamagable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage, attackPlayer);
        }

        if (impactEffect != null)
        {
            Vector3 newPosition = transform.position - transform.forward * 0.1f;
            ParticleSystem effect = Instantiate(
                impactEffect,
                newPosition,
                Quaternion.LookRotation(-transform.forward) // разворачиваем эффект от поверхности
            );
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (returnToPool != null)
        {
            returnToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
