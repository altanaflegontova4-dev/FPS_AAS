using System;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float moveSpeed, lifeTime;
    public Rigidbody rb;
    public GameObject impactEffect;
    // public bool damageEnemy, damagePlayer;
    // public int damageAmount;

    public bool attackPlayer;
    public int damage;

    private float lifeCounter;
    private Gun ownerGun;
    private bool hasHit;

    private Action<BulletController> returnToPool;

    public void SetReturnAction(Action<BulletController> returnAction)
    {
        returnToPool = returnAction;
    }

    public void Fire()
    {
        hasHit = false;
        lifeCounter = lifeTime;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = transform.forward * moveSpeed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        lifeCounter -= Time.deltaTime;

        if (lifeCounter <= 0)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        hasHit = true;

        IDamagable damageable = other.GetComponentInParent<IDamagable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage, attackPlayer);
        }

        if (impactEffect != null)
        {
            float offset = 0.7f;
            Vector3 newPosition = transform.position - transform.forward * offset;

            Instantiate(impactEffect, newPosition, transform.rotation);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (returnToPool != null)
        {
            returnToPool(this);//Send this bullet back to the pool that created it.
        }
        else
        {
            gameObject.SetActive(false);//a safety fallback, just in case it is not connected to the pool, just disable it
        }
    }
}